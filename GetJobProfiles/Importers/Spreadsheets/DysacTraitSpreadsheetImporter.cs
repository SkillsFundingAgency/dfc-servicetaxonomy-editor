using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Spreadsheet.DysacTrait;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers.Spreadsheets
{
    public class DysacTraitSpreadsheetImporter
    {
        // The DysacTrait dictionary of imported data in key/value format where the value is row model for the given spreadsheet
        public IReadOnlyDictionary<string, DysacTraitSpreadsheetRowModel> DysacTraitDictionary { get; private set; }

        // The name of the Spreadsheet to import
        private const string SpreadsheetName = "Trait";

        public IReadOnlyDictionary<string, DysacTraitSpreadsheetRowModel> Import(XSSFWorkbook spreadsheetWorkbook)
        {
            // Initialise the dictionary that will hold the Spreadsheet row/columns model
            var spreadsheetDictionary = new Dictionary<string, DysacTraitSpreadsheetRowModel>();

            // Get a reference to the Spreadsheet in the workbook
            var spreadsheet = spreadsheetWorkbook.GetSheet(SpreadsheetName);

            // Initialise the spreadsheet column index model to hold the columns indexes
            var spreadsheetColumnIndexModel = new DysacTraitSpreadsheetColumnIndexModel();

            // Initialise a spreadsheet row model so that we can use the column names to lookup the column indexes
            var spreadsheetRowModel = new DysacTraitSpreadsheetRowModel();

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
                    // Add the Spreadsheet to the dictionary
                    spreadsheetDictionary.Add(spreadsheetRowModel.ItemDefaultUrl.TrimStart('/'), spreadsheetRowModel);
                }
            }

            // Create a read-only version of the dictionary for public acsess to stop inadvertent changes to the data
            IReadOnlyDictionary<string, DysacTraitSpreadsheetRowModel> readonlySpreadsheet = spreadsheetDictionary;

            return readonlySpreadsheet;
        }

        private void InitialiseTheSpreadsheetColumnIndexes(
            NPOI.SS.UserModel.ISheet spreadsheet,
            DysacTraitSpreadsheetColumnIndexModel spreadsheetColumnIndexModel,
            DysacTraitSpreadsheetRowModel spreadsheetRowModel)
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

            // TitleColumnIndex
            spreadsheetColumnIndexModel.TitleColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Title))
                  .ColumnIndex;

            // JobProfileCategoriesColumnIndex
            spreadsheetColumnIndexModel.JobProfileCategoriesColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.JobProfileCategories).ToLower())
                  .ColumnIndex;

            // DescriptionColumnIndex
            spreadsheetColumnIndexModel.DescriptionColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Description))
                  .ColumnIndex;

            // ResultDisplayTextColumnIndex
            spreadsheetColumnIndexModel.ResultDisplayTextColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ResultDisplayText))
                  .ColumnIndex;
        }

        private DysacTraitSpreadsheetRowModel GetTheSpreadsheetRowData(
            NPOI.SS.UserModel.IRow spreadsheetRow,
            DysacTraitSpreadsheetColumnIndexModel spreadsheetColumnIndexModel)
        {
            DysacTraitSpreadsheetRowModel spreadsheetRowModel = null;

            if (spreadsheetRow != null)
            {
                spreadsheetRowModel = new DysacTraitSpreadsheetRowModel();

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

                // Title
                NPOI.SS.UserModel.ICell title = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.TitleColumnIndex);
                if (title != null)
                {
                    spreadsheetRowModel.Title = title.StringCellValue;
                }

                // JobProfileCategories
                NPOI.SS.UserModel.ICell JobProfileCategories = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.JobProfileCategoriesColumnIndex);
                if (JobProfileCategories != null)
                {
                    spreadsheetRowModel.JobProfileCategories = JobProfileCategories.StringCellValue;
                }

                // Description
                NPOI.SS.UserModel.ICell description = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.DescriptionColumnIndex);
                if (description != null)
                {
                    spreadsheetRowModel.Description = description.StringCellValue;
                }

                // ResultDisplayText
                NPOI.SS.UserModel.ICell ResultDisplayText = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ResultDisplayTextColumnIndex);
                if (ResultDisplayText != null)
                {
                    spreadsheetRowModel.ResultDisplayText = ResultDisplayText.StringCellValue;
                }
            }

            return spreadsheetRowModel;
        }
    }
}
