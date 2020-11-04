using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Fields.Factories;
using GetJobProfiles.Models.Recipe.Parts;
using GraphQL;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OrchardCore.Entities;

namespace GetJobProfiles.Importers
{
    public class DysacImporter
    {
        public DysacImporter(Dictionary<string, string> oNetToSocCodeDictionary, List<ONetOccupationalCodeContentItem> oNetOccupationalCodeContentItems)
        {
            _oNetToSocCodeDictionary = oNetToSocCodeDictionary;
            _oNetOccupationalCodeContentItems = oNetOccupationalCodeContentItems;
        }

        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();

        private static ContentPickerFactory ONetSkillsContentPickerFactory = new ContentPickerFactory();

        public IEnumerable<PersonalityTraitContentItem> PersonalityTraitContentItems { get; private set; }

        public IEnumerable<PersonalityShortQuestionContentItem> PersonalityShortQuestionContentItems { get; private set; }

        public IEnumerable<PersonalityQuestionSetContentItem> PersonalityQuestionSetContentItems { get; private set; }

        public string ONetSkillCypherCommands = string.Empty;

        private List<RelationshipModel> SkillToOccupationRelationships = new List<RelationshipModel>();

        private readonly ContentPickerFactory contentPickerFactory = new ContentPickerFactory();

        private readonly Dictionary<string, string> _oNetToSocCodeDictionary;

        private readonly Dictionary<string, List<ONetSkillRank>> _oNetOccupationToSkillRank = new Dictionary<string, List<ONetSkillRank>>();

        private readonly List<ONetOccupationalCodeContentItem> _oNetOccupationalCodeContentItems;

        private Dictionary<string, string> _personalityTraitContentItemDictionary { get; set; }

        private Dictionary<string, string> _personalityQuestionContentItemDictionary;

        internal void ImportTraits(Dictionary<string, string> jobCategoryDictionary, XSSFWorkbook dysacWorkbook, string timestamp)
        {
            var traits = ReadTraitsFromFile("Trait", dysacWorkbook);

            _personalityTraitContentItemDictionary = traits.Select(z => z.Title).Select(jc => new { Id = _generator.GenerateUniqueId(), Title = jc }).ToDictionary(y => y.Title, y => y.Id);

            PersonalityTraitContentItems = traits.Select(x => new PersonalityTraitContentItem(x.Title, timestamp, _personalityTraitContentItemDictionary[x.Title])
            {
                EponymousPart = new PersonalityTraitPart
                {
                    Description = new TextField(x.Description),
                    JobCategories = contentPickerFactory.CreateContentPickerFromContent("JobCategory", jobCategoryDictionary.Where(z => x.JobCategories.Select(y => y.ToLowerInvariant()).Contains(z.Key.ToLowerInvariant())).Select(k => k.Key.Trim()))
                }
            }).ToList();
        }

        internal void ImportShortQuestions(XSSFWorkbook dysacWorkbook, string timestamp)
        {
            var questions = ReadShortQuestionsFromFile("Shortquestion", dysacWorkbook);

            _personalityQuestionContentItemDictionary = questions.Select(x => x.Title).Select(jc => new { Id = _generator.GenerateUniqueId(), Title = jc }).ToDictionary(y => y.Title, y => y.Id);

            PersonalityShortQuestionContentItems = questions.Select(x => new PersonalityShortQuestionContentItem(x.Title, timestamp, _personalityQuestionContentItemDictionary[x.Title])
            {
                EponymousPart = new PersonalityShortQuestionPart
                {
                    Impact = new TextField(x.Impact.ToLowerInvariant() == "yes" ? "Negative" : "Positive"),
                    Trait = contentPickerFactory.CreateContentPickerFromContent("PersonalityTrait", new List<string> { x.Trait.Trim() })
                }
            }).ToList();
        }

        internal void ImportQuestionSet(string timestamp)
        {
            PersonalityQuestionSetContentItems = new List<PersonalityQuestionSetContentItem>
            {
                new PersonalityQuestionSetContentItem("Short Question Set", timestamp)
                {
                    EponymousPart = new PersonalityQuestionSetPart
                    {
                        Type = new TextField("short"),
                        Questions = contentPickerFactory.CreateContentPickerFromContent("PersonalityShortQuestion",  _personalityQuestionContentItemDictionary.Select(x=>x.Key.Trim()))
                    }
                }
            };
        }

        internal void ImportONetSkillRank(XSSFWorkbook jobProfileWorkbook)
        {
            LoadONetSkillRanks(jobProfileWorkbook);

            var sb = new StringBuilder();

            int totalSkillCount = _oNetOccupationToSkillRank.Sum(z => z.Value.Count);
            int currentSkill = 1;

            foreach (var occupation in _oNetOccupationToSkillRank)
            {
                foreach (var skill in occupation.Value)
                {
                    if (currentSkill != totalSkillCount)
                    {
                        sb.AppendLine($"\"match(o:ONetOccupationalCode{{ skos__prefLabel:'{occupation.Key}'}})-[r:hasONetSkill]-(d{{ skos__prefLabel:'{skill.Name}'}}) set r.ONetRank = {skill.Rank} return o\",");
                    }
                    else
                    {
                        sb.AppendLine($"\"match(o:ONetOccupationalCode{{ skos__prefLabel:'{occupation.Key}'}})-[r:hasONetSkill]-(d{{ skos__prefLabel:'{skill.Name}'}}) set r.ONetRank = {skill.Rank} return o\"");
                    }

                    currentSkill++;
                }
            }

            ONetSkillCypherCommands = sb.ToString();
        }

        private void LoadONetSkillRanks(XSSFWorkbook jobProfileWorkbook)
        {
            var sheet = jobProfileWorkbook.GetSheet("SocSkillsMatrix");

            var titleIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Title").ColumnIndex;
            var oNetIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ONetRank").ColumnIndex;

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);

                var title = row.GetCell(titleIndex).StringCellValue;
                var splitTitle = title.Split('-');

                var socCode = splitTitle[0].Substring(0, 4);
                var skillName = splitTitle[1];
                var rank = decimal.Parse(row.GetCell(oNetIndex).StringCellValue);

                var occupationalCode = _oNetToSocCodeDictionary.ContainsKey(socCode) ? _oNetToSocCodeDictionary[socCode].ToUpperInvariant() : string.Empty;

                if (!string.IsNullOrEmpty(occupationalCode))
                {
                    if (!_oNetOccupationToSkillRank.ContainsKey(occupationalCode))
                    {
                        _oNetOccupationToSkillRank.Add(occupationalCode, new List<ONetSkillRank> { new ONetSkillRank { Name = skillName, Rank = rank } });
                    }
                    else
                    {
                        if (!_oNetOccupationToSkillRank[occupationalCode].Any(z => z.Name.ToUpperInvariant().Trim() == skillName.ToUpperInvariant()))
                        {
                            _oNetOccupationToSkillRank[occupationalCode].Add(new ONetSkillRank { Name = skillName, Rank = rank });
                        }
                    }
                }
            }
        }

        private IEnumerable<PersonalityShortQuestion> ReadShortQuestionsFromFile(string sheetName, XSSFWorkbook dysacWorkbook)
        {
            var listToReturn = new List<PersonalityShortQuestion>();

            var sheet = dysacWorkbook.GetSheet(sheetName);

            var titleIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "QuestionText").ColumnIndex;
            var impactIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "IsNegative").ColumnIndex;
            var traitIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Trait").ColumnIndex;

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);

                var title = row.GetCell(titleIndex).StringCellValue;
                var impact = row.GetCell(impactIndex).StringCellValue;
                var trait = row.GetCell(traitIndex).StringCellValue;

                listToReturn.Add(new PersonalityShortQuestion { Title = title, Impact = impact, Trait = trait });
            }

            return listToReturn;
        }

        private IEnumerable<PersonalityTrait> ReadTraitsFromFile(string sheetName, XSSFWorkbook dysacWorkbook)
        {
            var listToReturn = new List<PersonalityTrait>();

            var sheet = dysacWorkbook.GetSheet(sheetName);

            var titleIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Title").ColumnIndex;
            var jobProfileCategoriesIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "jobprofilecategories").ColumnIndex;
            var resultDisplayTextIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ResultDisplayText").ColumnIndex;

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);

                var title = row.GetCell(titleIndex).StringCellValue;
                var jobCategories = row.GetCell(jobProfileCategoriesIndex).StringCellValue;
                var description = row.GetCell(resultDisplayTextIndex).StringCellValue;

                listToReturn.Add(new PersonalityTrait { Description = description, JobCategories = jobCategories.Split(',').ToList(), Title = title });
            }

            return listToReturn;
        }

        public void BuildONetOccupationalSkills(XSSFWorkbook mappingsWorkbook)
        {
            var sheet = mappingsWorkbook.GetSheet("JP Link");
            var dictionaryToReturn = new Dictionary<string, List<string>>();

            LoadONetSkills(sheet, dictionaryToReturn);

            foreach (var item in _oNetOccupationalCodeContentItems)
            {
                if (!string.IsNullOrEmpty(item.TitlePart.Title))
                {
                    var skillMapping = _oNetOccupationToSkillRank.ContainsKey(item.TitlePart.Title.ToUpperInvariant()) ? _oNetOccupationToSkillRank[item.TitlePart.Title.ToUpperInvariant()] : null;

                    var skillsToApply = dictionaryToReturn.ContainsKey(item.TitlePart.Title) ? dictionaryToReturn[item.TitlePart.Title] : new List<string>();

                    foreach (var skill in skillsToApply)
                    {
                        var selectedSkill = skillMapping?.FirstOrDefault(x => x.Name.ToUpperInvariant() == skill.ToUpperInvariant() || x.Name.ToUpperInvariant().Contains(skill.ToUpperInvariant()));

                        if (selectedSkill != null)
                        {
                            SkillToOccupationRelationships.Add(new RelationshipModel { Source = item.TitlePart.Title, Destination = selectedSkill.Name, Value = selectedSkill.Rank });
                        }
                        else
                        {
                            Console.WriteLine($"No skill mapping found for {skill} in {item.TitlePart.Title}");
                        }
                    }

                    if (skillsToApply.Any())
                    {
                        item.EponymousPart.ONetSkills = ONetSkillsContentPickerFactory.CreateContentPickerFromContent("ONetSkill", skillsToApply);
                    }
                }
            }
        }

        private void LoadONetSkills(ISheet sheet, Dictionary<string, List<string>> dictionaryToReturn)
        {
            //Skip first two rows
            for (int r = 2; r < sheet.PhysicalNumberOfRows; r++)
            {
                var row = sheet.GetRow(r);

                for (int c = 0; c < row.PhysicalNumberOfCells; c++)
                {
                    var socCode = row.Cells[c].StringCellValue.Substring(0, 5);
                    var value = row.Cells[c].StringCellValue.Replace("Published", "").Replace($"{socCode}-", "");

                    var nonPrefixedSocCode = socCode.Substring(0, 4);
                    var occupationalCode = _oNetToSocCodeDictionary.ContainsKey(nonPrefixedSocCode) ? _oNetToSocCodeDictionary[nonPrefixedSocCode] : string.Empty;

                    if (dictionaryToReturn.ContainsKey(occupationalCode))
                    {
                        if (!dictionaryToReturn[occupationalCode].Contains(value))
                        {
                            dictionaryToReturn[occupationalCode].Add(value);
                        }
                    }
                    else
                    {
                        dictionaryToReturn.Add(occupationalCode, new List<string> { value });
                    }
                }
            }
        }
    }
}
