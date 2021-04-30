using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Containers;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Builders
{
    public class JobProfileExcelWorkbookDataModelBuilder
    {
        public Dictionary<string, JobProfileExcelWorkbookColumnsDataModel> JobProfileExcelWorkbookColumnsDataModelDictionary
        {
            get;
            set;
        } = new Dictionary<string, JobProfileExcelWorkbookColumnsDataModel>();

        public Dictionary<string, JobProfileExcelWorkbookColumnsDataModel> Import(XSSFWorkbook workbook)
        {
            // Get a reference to the 'JobProfile' worksheet
            var jobProfileWorksheet = workbook.GetSheet("JobProfile");

            // Columns needed from the JobProfile worksheet to supplement the data from the JobProfile API are:
            // HiddenAlternativeTitle
            // DayToDayTasks
            // DigitalSkillsLevel
            // TitleOptions
            int hiddenAlternativeTitleColumnIndex = jobProfileWorksheet.GetRow(0).Cells.Single(x => x.StringCellValue == "HiddenAlternativeTitle").ColumnIndex;
            int dayToDayTasksColumnIndex = jobProfileWorksheet.GetRow(0).Cells.Single(x => x.StringCellValue == "WYDDayToDayTasks").ColumnIndex;
            int digitalSkillsLevelColumnIndex = jobProfileWorksheet.GetRow(0).Cells.Single(x => x.StringCellValue == "DigitalSkillsLevel").ColumnIndex;
            int dynamicTitleColumnIndex = jobProfileWorksheet.GetRow(0).Cells.Single(x => x.StringCellValue == "DynamicTitlePrefix").ColumnIndex;

            // key column (effectively the jp name)
            int urlColumnIndex = jobProfileWorksheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ItemDefaultUrl").ColumnIndex;

            //// Dictionary to hold the key and the JobProfileSpreadsheet model
            //var jobProfileSpreadsheetDictionary = new Dictionary<string, JobProfileExcelWorkbookColumnsModel>();

            // Initial jobProfileSpreadsheet model to hold the each cell of the spreadsheet for the needed columns (defined outside of the loop so it can be reused if we skip any blank rows)
            var jobProfileSpreadsheet = new JobProfileExcelWorkbookColumnsDataModel();

            // iterate the rows in the spreadsheet
            for (int i = 1; i <= jobProfileWorksheet.LastRowNum; ++i)
            {
                // get the row data
                var row = jobProfileWorksheet.GetRow(i);

                // get the individual cell data
                jobProfileSpreadsheet.HiddenAlternativeTitle = row.GetCell(hiddenAlternativeTitleColumnIndex).StringCellValue;
                jobProfileSpreadsheet.DayToDayTasks = row.GetCell(dayToDayTasksColumnIndex).StringCellValue;
                jobProfileSpreadsheet.DigitalSkillsLevel = row.GetCell(digitalSkillsLevelColumnIndex).StringCellValue;
                jobProfileSpreadsheet.TitleOptions = row.GetCell(dynamicTitleColumnIndex).StringCellValue;

                // if all of the cells are blank/empty then don't bother storing the row
                if (string.IsNullOrEmpty(jobProfileSpreadsheet.HiddenAlternativeTitle) &&
                    string.IsNullOrEmpty(jobProfileSpreadsheet.DayToDayTasks) &&
                    string.IsNullOrEmpty(jobProfileSpreadsheet.DigitalSkillsLevel) &&
                    string.IsNullOrEmpty(jobProfileSpreadsheet.TitleOptions))
                {
                    continue;
                }

                // get the cell data for the key
                string url = row.GetCell(urlColumnIndex).StringCellValue.TrimStart('/');

                // add the jobProfileSpreadsheet to the dictionary
                JobProfileExcelWorkbookColumnsDataModelDictionary.Add(url, jobProfileSpreadsheet);

                // new jobProfileSpreadsheet model to hold the each cell of the spreadsheet for the needed columns
                jobProfileSpreadsheet = new JobProfileExcelWorkbookColumnsDataModel();
            }

            return JobProfileExcelWorkbookColumnsDataModelDictionary;
        }
    }
}
