using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Spreadsheet.JobProfileSoc;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers.Spreadsheets
{
    public class JobProfileSocSpreadsheetImporter
    {
        // The JobProfileSoc dictionary of imported data in key/value format where the value is row model for the given spreadsheet
        public IReadOnlyDictionary<string, JobProfileSocSpreadsheetRowModel> JobProfileSocDictionary { get; private set; }

        // The name of the Spreadsheet to import
        private const string SpreadsheetName = "JobProfileSoc";

        public IReadOnlyDictionary<string, JobProfileSocSpreadsheetRowModel> Import(XSSFWorkbook spreadsheetWorkbook)
        {
            // Initialise the dictionary that will hold the Spreadsheet row/columns model
            var spreadsheetDictionary = new Dictionary<string, JobProfileSocSpreadsheetRowModel>();

            // Get a reference to the Spreadsheet in the workbook
            var spreadsheet = spreadsheetWorkbook.GetSheet(SpreadsheetName);

            // Initialise the spreadsheet column index model to hold the columns indexes
            var spreadsheetColumnIndexModel = new JobProfileSocSpreadsheetColumnIndexModel();

            // Initialise a spreadsheet row model so that we can use the column names to lookup the column indexes
            var spreadsheetRowModel = new JobProfileSocSpreadsheetRowModel();

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
            IReadOnlyDictionary<string, JobProfileSocSpreadsheetRowModel> readonlySpreadsheet = spreadsheetDictionary;

            return readonlySpreadsheet;
        }

        private void InitialiseTheSpreadsheetColumnIndexes(
            NPOI.SS.UserModel.ISheet spreadsheet,
            JobProfileSocSpreadsheetColumnIndexModel spreadsheetColumnIndexModel,
            JobProfileSocSpreadsheetRowModel spreadsheetRowModel)
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

            // ApprenticeshipStandardsColumnIndex
            spreadsheetColumnIndexModel.ApprenticeshipStandardsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ApprenticeshipStandards).ToLower())
                  .ColumnIndex;

            // DescriptionColumnIndex
            spreadsheetColumnIndexModel.DescriptionColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Description))
                  .ColumnIndex;

            // SOCCodeColumnIndex
            spreadsheetColumnIndexModel.SOCCodeColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.SOCCode))
                  .ColumnIndex;

            // ONetOccupationalCodeColumnIndex
            spreadsheetColumnIndexModel.ONetOccupationalCodeColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ONetOccupationalCode))
                  .ColumnIndex;

            // ApprenticeshipFrameworksColumnIndex
            spreadsheetColumnIndexModel.ApprenticeshipFrameworksColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ApprenticeshipFrameworks).ToLower())
                  .ColumnIndex;
        }

        private JobProfileSocSpreadsheetRowModel GetTheSpreadsheetRowData(
            NPOI.SS.UserModel.IRow spreadsheetRow,
            JobProfileSocSpreadsheetColumnIndexModel spreadsheetColumnIndexModel)
        {
            JobProfileSocSpreadsheetRowModel spreadsheetRowModel = null;

            if (spreadsheetRow != null)
            {
                spreadsheetRowModel = new JobProfileSocSpreadsheetRowModel();

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

                // ApprenticeshipStandards
                NPOI.SS.UserModel.ICell apprenticeshipStandards = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ApprenticeshipStandardsColumnIndex);
                if (apprenticeshipStandards != null)
                {
                    spreadsheetRowModel.ApprenticeshipStandards = apprenticeshipStandards.StringCellValue;
                }

                // Description
                NPOI.SS.UserModel.ICell description = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.DescriptionColumnIndex);
                if (description != null)
                {
                    spreadsheetRowModel.Description = description.StringCellValue;
                }

                // SOCCode
                NPOI.SS.UserModel.ICell sOCCode = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.SOCCodeColumnIndex);
                if (sOCCode != null)
                {
                    spreadsheetRowModel.SOCCode = sOCCode.StringCellValue;
                }

                // ONetOccupationalCode
                NPOI.SS.UserModel.ICell oNetOccupationalCode = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ONetOccupationalCodeColumnIndex);
                if (oNetOccupationalCode != null)
                {
                    spreadsheetRowModel.ONetOccupationalCode = oNetOccupationalCode.StringCellValue;
                }

                // ONetOccupationalCode
                NPOI.SS.UserModel.ICell apprenticeshipFrameworks = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ApprenticeshipFrameworksColumnIndex);
                if (apprenticeshipFrameworks != null)
                {
                    spreadsheetRowModel.ApprenticeshipFrameworks = apprenticeshipFrameworks.StringCellValue;
                }
            }

            return spreadsheetRowModel;
        }
    }
}
