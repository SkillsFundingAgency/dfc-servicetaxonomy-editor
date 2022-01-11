using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Spreadsheet.DysacShortQuestion;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers.Spreadsheets
{
    public class DysacShortQuestionSpreadsheetImporter
    {
        // The DysacShortQuestion dictionary of imported data in key/value format where the value is row model for the given spreadsheet
        public IReadOnlyDictionary<string, DysacShortQuestionSpreadsheetRowModel> DysacShortQuestionDictionary { get; private set; }

        // The name of the Spreadsheet to import
        private const string SpreadsheetName = "ShortQuestion";

        public IReadOnlyDictionary<string, DysacShortQuestionSpreadsheetRowModel> Import(XSSFWorkbook spreadsheetWorkbook)
        {
            // Initialise the dictionary that will hold the Spreadsheet row/columns model
            var spreadsheetDictionary = new Dictionary<string, DysacShortQuestionSpreadsheetRowModel>();

            // Get a reference to the Spreadsheet in the workbook
            var spreadsheet = spreadsheetWorkbook.GetSheet(SpreadsheetName);

            // Initialise the spreadsheet column index model to hold the columns indexes
            var spreadsheetColumnIndexModel = new DysacShortQuestionSpreadsheetColumnIndexModel();

            // Initialise a spreadsheet row model so that we can use the column names to lookup the column indexes
            var spreadsheetRowModel = new DysacShortQuestionSpreadsheetRowModel();

            // Initialise the column index values
            InitialiseTheSpreadsheetColumnIndexes(spreadsheet, spreadsheetColumnIndexModel, spreadsheetRowModel);

            // Iterate through the rows in the spreadsheet to get the row data
            for (int i = 1; i <= spreadsheet.LastRowNum; ++i)
            {
                // Get the row data
                var spreadsheetRow = spreadsheet.GetRow(i);

                // Get the row data (first time through we are discarding the socCodesSpreadsheetRowModel model used above which is used to get the column indexes)
                spreadsheetRowModel = GetTheSpreadsheetRowData(spreadsheetRow, spreadsheetColumnIndexModel);

                // Ignore blank rows
                if (spreadsheetRowModel != null)
                {
                    // add the Spreadsheet to the dictionary
                    spreadsheetDictionary.Add(spreadsheetRowModel.QuestionText, spreadsheetRowModel);
                }
            }

            // Create a read-only version of the dictionary for public acsess to stop inadvertent changes to the data
            IReadOnlyDictionary<string, DysacShortQuestionSpreadsheetRowModel> readonlySpreadsheet = spreadsheetDictionary;

            return readonlySpreadsheet;
        }

        private void InitialiseTheSpreadsheetColumnIndexes(
            NPOI.SS.UserModel.ISheet spreadsheet,
            DysacShortQuestionSpreadsheetColumnIndexModel spreadsheetColumnIndexModel,
            DysacShortQuestionSpreadsheetRowModel spreadsheetRowModel)
        {
            // PublicationDateColumnIndex
            spreadsheetColumnIndexModel.PublicationDateColumnIndex = spreadsheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.PublicationDate))
                .ColumnIndex;

            // IsNegativeColumnIndex
            spreadsheetColumnIndexModel.IsNegativeColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.IsNegative))
                  .ColumnIndex;

            // QuestionTextColumnIndex
            spreadsheetColumnIndexModel.QuestionTextColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.QuestionText))
                  .ColumnIndex;

            // TraitColumnIndex
            spreadsheetColumnIndexModel.TraitColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Trait))
                  .ColumnIndex;
        }

        private DysacShortQuestionSpreadsheetRowModel GetTheSpreadsheetRowData(
            NPOI.SS.UserModel.IRow spreadsheetRow,
            DysacShortQuestionSpreadsheetColumnIndexModel spreadsheetColumnIndexModel)
        {
            DysacShortQuestionSpreadsheetRowModel spreadsheetRowModel = null;

            if (spreadsheetRow != null)
            {
                spreadsheetRowModel = new DysacShortQuestionSpreadsheetRowModel();

                // PublicationDate
                NPOI.SS.UserModel.ICell publicationDate = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.PublicationDateColumnIndex);
                if (publicationDate != null)
                {
                    spreadsheetRowModel.PublicationDate = Convert.ToDateTime(publicationDate.StringCellValue);
                }

                // IsNegative
                NPOI.SS.UserModel.ICell isNegative = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.IsNegativeColumnIndex);
                if (isNegative != null)
                {
                    spreadsheetRowModel.IsNegative = Convert.ToBoolean(isNegative.StringCellValue);
                }

                // QuestionText
                NPOI.SS.UserModel.ICell questionText = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.QuestionTextColumnIndex);
                if (questionText != null)
                {
                    spreadsheetRowModel.QuestionText = questionText.StringCellValue;
                }

                // Trait
                NPOI.SS.UserModel.ICell trait = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.TraitColumnIndex);
                if (trait != null)
                {
                    spreadsheetRowModel.Trait = trait.StringCellValue;
                }
            }

            return spreadsheetRowModel;
        }
    }
}
