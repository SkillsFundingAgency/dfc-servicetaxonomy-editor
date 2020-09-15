using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;
using GraphQL;
using NPOI.XSSF.UserModel;
using OrchardCore.Entities;

namespace GetJobProfiles.Importers
{
    public class DysacImporter
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();

        public IEnumerable<PersonalityTraitContentItem> PersonalityTraitContentItems { get; private set; }
        public IEnumerable<PersonalityShortQuestionContentItem> PersonalityShortQuestionContentItems { get; private set; }

        private Dictionary<string, string> _personalityTraitContentItemIdDictionary { get; set; }
      
        internal void ImportTraits(Dictionary<string,string> jobCategoryDictionary, XSSFWorkbook dysacWorkbook, string timestamp)
        {
            var traits = ReadTraitsFromFile("Trait", dysacWorkbook);

            _personalityTraitContentItemIdDictionary = traits.Select(z => z.Title).Select(jc => new { Id = _generator.GenerateUniqueId(), Title = jc }).ToDictionary(y => y.Title, y => y.Id);

            PersonalityTraitContentItems = traits.Select(x => new PersonalityTraitContentItem(x.Title, timestamp, _personalityTraitContentItemIdDictionary[x.Title])
            {
                EponymousPart = new PersonalityTraitPart
                {
                    Description = new TextField(x.Description),
                    JobCategories = new ContentPicker
                    {
                        ContentItemIds = jobCategoryDictionary.Where(z => x.JobCategories.Select(y => y.ToLowerInvariant()).Contains(z.Key.ToLowerInvariant())).Select(k => k.Value)
                    }
                }
            }).ToList();
        }

        internal void ImportShortQuestions(XSSFWorkbook dysacWorkbook, string timestamp)
        {
            var questions = ReadShortQuestionsFromFile("Shortquestion", dysacWorkbook);

            PersonalityShortQuestionContentItems = questions.Select(x => new PersonalityShortQuestionContentItem(x.Title, timestamp)
            {
                EponymousPart = new PersonalityShortQuestionPart
                {
                    Impact = new TextField(x.Impact.ToLowerInvariant() == "yes" ? "Negative" : "Positive"),
                    Trait = new ContentPicker
                    {
                        ContentItemIds = new List<string> { _personalityTraitContentItemIdDictionary[x.Trait] }
                    }
                }
            }).ToList();
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

        public Dictionary<string, List<string>> GetSocToPersonalitySkillMappings(XSSFWorkbook mappingsWorkbook)
        {
            var sheet = mappingsWorkbook.GetSheet("bla");

            for (int r = 0; r < sheet.PhysicalNumberOfRows; r++)
            {
                var row = sheet.GetRow(r);

                for (int c = 0; c < row.PhysicalNumberOfCells; c++)
                {
                    var socCode = row.GetCell(c).StringCellValue.Substring(0, 4);
                }
            }

            return new Dictionary<string, List<string>>();
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
    }
}
