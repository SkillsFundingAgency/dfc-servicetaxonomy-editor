using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers
{
    public class DysacImporter
    {
        internal void Import(IEnumerable<JobCategoryContentItem> jobCategoryContentItems, XSSFWorkbook dysacWorkbook)
        {
            var traits = ReadFromFile("Trait", dysacWorkbook);
        }

        private IEnumerable<DysacTrait> ReadFromFile(string sheetName, XSSFWorkbook jobProfileWorkbook)
        {
            var listToReturn = new List<DysacTrait>();

            var sheet = jobProfileWorkbook.GetSheet(sheetName);

            var titleIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Title").ColumnIndex;
            var jobProfileCategoriesIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "jobprofilecategories").ColumnIndex;
            var resultDisplayTextIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ResultDisplayText").ColumnIndex;

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);

                var title = row.GetCell(titleIndex).StringCellValue;
                var jobCategories = row.GetCell(jobProfileCategoriesIndex).StringCellValue;
                var description = row.GetCell(resultDisplayTextIndex).StringCellValue;

                listToReturn.Add(new DysacTrait { Description = description, JobCategories = jobCategories.Split(',').ToList(), Title = title });
            }

            return listToReturn;
        }
    }
