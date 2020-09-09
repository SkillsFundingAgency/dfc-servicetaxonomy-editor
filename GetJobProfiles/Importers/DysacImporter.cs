using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;
using GraphQL;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers
{
    public class DysacImporter
    {
        public IEnumerable<PersonalityTraitContentItem> PersonalityTraitContentItems { get; private set; }

        internal void Import(Dictionary<string,string> jobCategoryDictionary, XSSFWorkbook dysacWorkbook, string timestamp)
        {
            var traits = ReadFromFile("Trait", dysacWorkbook);

            PersonalityTraitContentItems = traits.Select(x => new PersonalityTraitContentItem(x.Title, timestamp)
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

        private IEnumerable<PersonalityTrait> ReadFromFile(string sheetName, XSSFWorkbook dysacWorkbook)
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
