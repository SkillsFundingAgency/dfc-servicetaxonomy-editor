using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Spreadsheet.ApprenticeshipLink;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers.Spreadsheets
{
    public class ApprenticeshipLinkSpreadsheetImporter
    {
        // The ApprenticeshipLink dictionary of imported data in key/value format where the value is row model for the given spreadsheet
        public IReadOnlyDictionary<string, ApprenticeshipLinkSpreadsheetRowModel> ApprenticeshipLinkDictionary { get; private set; }

        // The name of the Spreadsheet to import
        private const string SpreadsheetName = "ApprenticeshipLink";

        public IReadOnlyDictionary<string, ApprenticeshipLinkSpreadsheetRowModel> Import(XSSFWorkbook spreadsheetWorkbook)
        {
            // Initialise the dictionary that will hold the Spreadsheet row/columns model
            var spreadsheetDictionary = new Dictionary<string, ApprenticeshipLinkSpreadsheetRowModel>();

            // Get a reference to the Spreadsheet in the workbook
            var spreadsheet = spreadsheetWorkbook.GetSheet(SpreadsheetName);

            // Initialise the spreadsheet column index model to hold the columns indexes
            var spreadsheetColumnIndexModel = new ApprenticeshipLinkSpreadsheetColumnIndexModel();

            // Initialise a spreadsheet row model so that we can use the column names to lookup the column indexes
            var spreadsheetRowModel = new ApprenticeshipLinkSpreadsheetRowModel();

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
                    // Add the row to the dictionary
                    spreadsheetDictionary.Add(spreadsheetRowModel.ItemDefaultUrl.TrimStart('/'), spreadsheetRowModel);
                }
            }

            // Create a read-only version of the dictionary for public access to stop inadvertent changes to the data
            IReadOnlyDictionary<string, ApprenticeshipLinkSpreadsheetRowModel> readonlySpreadsheet = spreadsheetDictionary;

            return readonlySpreadsheet;
        }

        private void InitialiseTheSpreadsheetColumnIndexes(
            NPOI.SS.UserModel.ISheet spreadsheet,
            ApprenticeshipLinkSpreadsheetColumnIndexModel spreadsheetColumnIndexModel,
            ApprenticeshipLinkSpreadsheetRowModel spreadsheetRowModel)
        {
            // SystemParentIdColumnIndex
            spreadsheetColumnIndexModel.SystemParentIdColumnIndex = spreadsheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.SystemParentId))
                .ColumnIndex;

            // IncludeInSitemapColumnIndex
            spreadsheetColumnIndexModel.IncludeInSitemapColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.IncludeInSitemap))
                  .ColumnIndex;

            // IdColumnIndex
            spreadsheetColumnIndexModel.IdColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Id))
                  .ColumnIndex;

            // DateCreatedColumnIndex
            spreadsheetColumnIndexModel.DateCreatedColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.DateCreated))
                  .ColumnIndex;

            // ItemDefaultUrlColumnIndex
            spreadsheetColumnIndexModel.ItemDefaultUrlColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ItemDefaultUrl))
                  .ColumnIndex;

            // UrlNameColumnIndex
            spreadsheetColumnIndexModel.UrlNameColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.UrlName))
                  .ColumnIndex;

            // PublicationDateColumnIndex
            spreadsheetColumnIndexModel.PublicationDateColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.PublicationDate))
                  .ColumnIndex;

            // UrlColumnIndex
            spreadsheetColumnIndexModel.UrlColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Url))
                  .ColumnIndex;

            // TitleColumnIndex
            spreadsheetColumnIndexModel.TitleColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Title))
                  .ColumnIndex;

            // TextColumnIndex
            spreadsheetColumnIndexModel.TextColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Text))
                  .ColumnIndex;
        }

        private ApprenticeshipLinkSpreadsheetRowModel GetTheSpreadsheetRowData(
            NPOI.SS.UserModel.IRow spreadsheetRow,
            ApprenticeshipLinkSpreadsheetColumnIndexModel spreadsheetColumnIndexModel)
        {
            ApprenticeshipLinkSpreadsheetRowModel spreadsheetRowModel = null;

            if (spreadsheetRow != null)
            {
                spreadsheetRowModel = new ApprenticeshipLinkSpreadsheetRowModel();

                // SystemParentId
                NPOI.SS.UserModel.ICell systemParentId = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.SystemParentIdColumnIndex);
                if (systemParentId != null)
                {
                    spreadsheetRowModel.SystemParentId = systemParentId.StringCellValue;
                }

                // IncludeInSitemap
                NPOI.SS.UserModel.ICell includeInSitemap = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.IncludeInSitemapColumnIndex);
                if (includeInSitemap != null)
                {
                    spreadsheetRowModel.IncludeInSitemap = Convert.ToBoolean(includeInSitemap.StringCellValue);
                }

                // Id
                NPOI.SS.UserModel.ICell id = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.IdColumnIndex);
                if (id != null)
                {
                    spreadsheetRowModel.Id = id.StringCellValue;
                }

                // DateCreated
                NPOI.SS.UserModel.ICell dateCreated = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.DateCreatedColumnIndex);
                if (dateCreated != null)
                {
                    spreadsheetRowModel.DateCreated = Convert.ToDateTime(dateCreated.StringCellValue);
                }

                // ItemDefaultUrl
                NPOI.SS.UserModel.ICell itemDefaultUrl = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ItemDefaultUrlColumnIndex);
                if (itemDefaultUrl != null)
                {
                    spreadsheetRowModel.ItemDefaultUrl = itemDefaultUrl.StringCellValue;
                }

                // UrlName
                NPOI.SS.UserModel.ICell urlName = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.UrlNameColumnIndex);
                if (urlName != null)
                {
                    spreadsheetRowModel.UrlName = urlName.StringCellValue;
                }

                // PublicationDate
                NPOI.SS.UserModel.ICell publicationDate = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.PublicationDateColumnIndex);
                if (publicationDate != null)
                {
                    spreadsheetRowModel.PublicationDate = Convert.ToDateTime(publicationDate.StringCellValue);
                }

                // Url
                NPOI.SS.UserModel.ICell url = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.UrlColumnIndex);
                if (url != null)
                {
                    spreadsheetRowModel.Url = url.StringCellValue;
                }

                // Title
                NPOI.SS.UserModel.ICell title = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.TitleColumnIndex);
                if (title != null)
                {
                    spreadsheetRowModel.Title = title.StringCellValue;
                }

                // Text
                NPOI.SS.UserModel.ICell Text = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.TextColumnIndex);
                if (Text != null)
                {
                    spreadsheetRowModel.Text = Text.StringCellValue;
                }
            }

            return spreadsheetRowModel;
        }
    }
}
