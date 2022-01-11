using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Spreadsheet.Skill;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers.Spreadsheets
{
    public class SkillSpreadsheetImporter
    {
        // The Skill dictionary of imported data in key/value format where the value is row model for the given spreadsheet
        public IReadOnlyDictionary<string, SkillSpreadsheetRowModel> SkillDictionary { get; private set; }

        // The name of the Spreadsheet to import
        private const string SpreadsheetName = "Skill";

        public IReadOnlyDictionary<string, SkillSpreadsheetRowModel> Import(XSSFWorkbook spreadsheetWorkbook)
        {
            // Initialise the dictionary that will hold the Spreadsheet row/columns model
            var spreadsheetDictionary = new Dictionary<string, SkillSpreadsheetRowModel>();

            // Get a reference to the Spreadsheet in the workbook
            var spreadsheet = spreadsheetWorkbook.GetSheet(SpreadsheetName);

            // Initialise the spreadsheet column index model to hold the columns indexes
            var spreadsheetColumnIndexModel = new SkillSpreadsheetColumnIndexModel();

            // Initialise a spreadsheet row model so that we can use the column names to lookup the column indexes
            var spreadsheetRowModel = new SkillSpreadsheetRowModel();

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
            IReadOnlyDictionary<string, SkillSpreadsheetRowModel> readonlySpreadsheet = spreadsheetDictionary;

            return readonlySpreadsheet;
        }

        private void InitialiseTheSpreadsheetColumnIndexes(
            NPOI.SS.UserModel.ISheet spreadsheet,
            SkillSpreadsheetColumnIndexModel spreadsheetColumnIndexModel,
            SkillSpreadsheetRowModel spreadsheetRowModel)
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

            // ONetAttributeTypeColumnIndex
            spreadsheetColumnIndexModel.TitleColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Title))
                  .ColumnIndex;

            // PSFDescriptionColumnIndex
            spreadsheetColumnIndexModel.PSFDescriptionColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.PSFDescription))
                  .ColumnIndex;

            // ContextualisedColumnIndex
            spreadsheetColumnIndexModel.PSFHiddenColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.PSFHidden))
                  .ColumnIndex;

            // DescriptionColumnIndex
            spreadsheetColumnIndexModel.DescriptionColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Description))
                  .ColumnIndex;

            // ONetElementIdColumnIndex
            spreadsheetColumnIndexModel.ONetElementIdColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ONetElementId))
                  .ColumnIndex;

            // PSFCategoriesColumnIndex
            spreadsheetColumnIndexModel.PSFCategoriesColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.PSFCategories))
                  .ColumnIndex;

            // PSFNotApplicableColumnIndex
            spreadsheetColumnIndexModel.PSFNotApplicableColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.PSFNotApplicable))
                  .ColumnIndex;

            // PSFLabelColumnIndex
            spreadsheetColumnIndexModel.PSFLabelColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.PSFLabel))
                  .ColumnIndex;

            // PSFOrderColumnIndex
            spreadsheetColumnIndexModel.PSFOrderColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.PSFOrder))
                  .ColumnIndex;
        }

        private SkillSpreadsheetRowModel GetTheSpreadsheetRowData(
            NPOI.SS.UserModel.IRow spreadsheetRow,
            SkillSpreadsheetColumnIndexModel spreadsheetColumnIndexModel)
        {
            SkillSpreadsheetRowModel spreadsheetRowModel = null;

            if (spreadsheetRow != null)
            {
                spreadsheetRowModel = new SkillSpreadsheetRowModel();

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

                // PSFDescription
                NPOI.SS.UserModel.ICell pSFDescription = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.PSFDescriptionColumnIndex);
                if (pSFDescription != null)
                {
                    spreadsheetRowModel.PSFDescription = pSFDescription.StringCellValue;
                }

                // PSFHidden
                NPOI.SS.UserModel.ICell pSFHidden = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.PSFHiddenColumnIndex);
                if (pSFHidden != null)
                {
                    spreadsheetRowModel.PSFHidden = Convert.ToBoolean(pSFHidden.StringCellValue);
                }

                // Description
                NPOI.SS.UserModel.ICell description = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.DescriptionColumnIndex);
                if (description != null)
                {
                    spreadsheetRowModel.Description = description.StringCellValue;
                }

                // ONetElementId
                NPOI.SS.UserModel.ICell oNetElementId = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ONetElementIdColumnIndex);
                if (oNetElementId != null)
                {
                    spreadsheetRowModel.ONetElementId = oNetElementId.StringCellValue;
                }

                // PSFCategories
                NPOI.SS.UserModel.ICell pSFCategories = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.PSFCategoriesColumnIndex);
                if (pSFCategories != null)
                {
                    spreadsheetRowModel.PSFCategories = pSFCategories.StringCellValue;
                }

                // PSFNotApplicable
                NPOI.SS.UserModel.ICell pSFNotApplicable = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.PSFNotApplicableColumnIndex);
                if (pSFNotApplicable != null)
                {
                    spreadsheetRowModel.PSFNotApplicable = Convert.ToBoolean(pSFNotApplicable.StringCellValue);
                }

                // PSFLabel
                NPOI.SS.UserModel.ICell pSFLabel = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.PSFLabelColumnIndex);
                if (pSFLabel != null)
                {
                    spreadsheetRowModel.PSFLabel = pSFLabel.StringCellValue;
                }

                // PSFOrder
                NPOI.SS.UserModel.ICell pSFOrder = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.PSFOrderColumnIndex);
                if (pSFOrder != null)
                {
                    spreadsheetRowModel.PSFOrder = pSFOrder.StringCellValue;
                }
            }

            return spreadsheetRowModel;
        }
    }
}
