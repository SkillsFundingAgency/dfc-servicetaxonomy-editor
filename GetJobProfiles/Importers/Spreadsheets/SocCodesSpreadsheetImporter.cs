using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Spreadsheet.SocCodes;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers.Spreadsheets
{
    class SocCodesSpreadsheetImporter
    {
        // The SocCodes dictionary of imported data in key/value format where the value is row model for the given spreadsheet
        public IReadOnlyDictionary<int, SocCodesSpreadsheetRowModel> SocCodesDictionary { get; private set; }

        // The name of the Spreadsheet to import
        private const string SpreadsheetName = "soc_codes";

        public IReadOnlyDictionary<int, SocCodesSpreadsheetRowModel> Import(XSSFWorkbook spreadsheetWorkbook)
        {
            // Initialise the dictionary that will hold the Spreadsheet row/columns model
            var spreadsheetDictionary = new Dictionary<int, SocCodesSpreadsheetRowModel>();

            // Get a reference to the Spreadsheet in the workbook
            var spreadsheet = spreadsheetWorkbook.GetSheet(SpreadsheetName);

            // Initialise the spreadsheet column index model to hold the columns indexes
            var spreadsheetColumnIndexModel = new SocCodesSpreadsheetColumnIndexModel();

            // Initialise a spreadsheet row model so that we can use the column names to lookup the column indexes
            var spreadsheetRowModel = new SocCodesSpreadsheetRowModel();

            // Initialise the column index values
            InitialiseTheSpreadsheetColumnIndexes(spreadsheet, spreadsheetColumnIndexModel, spreadsheetRowModel);

            // Iterate through the rows in the spreadsheet to get the row data
            for (int i = 1; i <= spreadsheet.LastRowNum; ++i)
            {
                // Get the row data
                var spreadsheetRow = spreadsheet.GetRow(i);

                // Get the row data (first time through we are discarding the socCodesSpreadsheetRowModel model used above which is used to get the column indexes)
                spreadsheetRowModel = GetTheSpreadsheetRowData(spreadsheetColumnIndexModel, spreadsheetRow);

                // Ignore blank rows
                if(spreadsheetRowModel != null)
                {
                    // Ignore blank Unit cells
                    if (spreadsheetRowModel.Unit != null)
                    {
                        // Add the row to the dictionary
                        spreadsheetDictionary.Add(spreadsheetRowModel.Unit.Value, spreadsheetRowModel);
                    }
                }
            }

            // Create a read-only version of the dictionary for public use to stop inadvertent changes to the data
            IReadOnlyDictionary<int, SocCodesSpreadsheetRowModel> readonlySpreadsheet = spreadsheetDictionary;

            return readonlySpreadsheet;
        }

        private void InitialiseTheSpreadsheetColumnIndexes(
            NPOI.SS.UserModel.ISheet spreadsheet,
            SocCodesSpreadsheetColumnIndexModel spreadsheetColumnIndexModel,
            SocCodesSpreadsheetRowModel spreadsheetRowModel)
        {
            // UnitColumnIndex
            spreadsheetColumnIndexModel.UnitColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Unit))
                  .ColumnIndex;

            // TitleColumnIndex
            spreadsheetColumnIndexModel.TitleColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Title))
                  .ColumnIndex;
        }

        private SocCodesSpreadsheetRowModel GetTheSpreadsheetRowData(
            SocCodesSpreadsheetColumnIndexModel spreadsheetColumnIndexModel,
            NPOI.SS.UserModel.IRow spreadsheetRow)
        {
            SocCodesSpreadsheetRowModel spreadsheetRowModel = null;

            if(spreadsheetRow != null)
            {
                spreadsheetRowModel = new SocCodesSpreadsheetRowModel();

                // Unit
                NPOI.SS.UserModel.ICell unit = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.UnitColumnIndex);
                if (unit != null)
                {
                    spreadsheetRowModel.Unit = Convert.ToInt32(unit.NumericCellValue);
                }

                // Title
                NPOI.SS.UserModel.ICell title = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.TitleColumnIndex);
                if (title != null)
                {
                    spreadsheetRowModel.Title = title.StringCellValue;
                }
            }

            return spreadsheetRowModel;
        }
    }
}
