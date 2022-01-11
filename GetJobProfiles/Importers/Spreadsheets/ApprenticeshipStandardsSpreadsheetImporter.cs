using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Spreadsheet.ApprenticeshipStandards;
using NPOI.XSSF.UserModel;

namespace GetApprenticeshipStandardss.Importers.Spreadsheets
{
    public class ApprenticeshipStandardsSpreadsheetImporter
    {
        // The ApprenticeshipStandards dictionary of imported data in key/value format where the value is row model for the given spreadsheet
        public IReadOnlyDictionary<string, ApprenticeshipStandardsSpreadsheetRowModel> ApprenticeshipStandardsDictionary { get; private set; }

        // The name of the Spreadsheet to import
        private const string SpreadsheetName = "ApprenticeshipStandards";

        public IReadOnlyDictionary<string, ApprenticeshipStandardsSpreadsheetRowModel> Import(XSSFWorkbook spreadsheetWorkbook)
        {
            // Initialise the dictionary that will hold the Spreadsheet row/columns model
            var spreadsheetDictionary = new Dictionary<string, ApprenticeshipStandardsSpreadsheetRowModel>();

            // Get a reference to the Spreadsheet in the workbook
            var spreadsheet = spreadsheetWorkbook.GetSheet(SpreadsheetName);

            // Initialise the spreadsheet column index model to hold the columns indexes
            var spreadsheetColumnIndexModel = new ApprenticeshipStandardsSpreadsheetColumnIndexModel();

            // Initialise a spreadsheet row model so that we can use the column names to lookup the column indexes
            var spreadsheetRowModel = new ApprenticeshipStandardsSpreadsheetRowModel();

            // Initialise the column index values
            InitialiseTheSpreadsheetColumnIndexes(spreadsheet, spreadsheetColumnIndexModel, spreadsheetRowModel);

            // Iterate through the rows in the spreadsheet to get the row data
            for (int i = 1; i <= spreadsheet.LastRowNum; ++i)
            {
                // Get the row data
                var spreadsheetRow = spreadsheet.GetRow(i);

                // Get the individual cell data
                spreadsheetRowModel = GetTheSpreadsheetRowData(spreadsheetRow, spreadsheetColumnIndexModel);

                // Ignore blank rows
                if (spreadsheetRowModel != null)
                {
                    // Add the Spreadsheet to the dictionary
                    spreadsheetDictionary.Add(spreadsheetRowModel.Name.Trim(), spreadsheetRowModel);
                }
            }

            // Create a read-only version of the dictionary for public access to stop inadvertent changes to the data
            IReadOnlyDictionary<string, ApprenticeshipStandardsSpreadsheetRowModel> readonlySpreadsheet = spreadsheetDictionary;

            return readonlySpreadsheet;
        }

        private void InitialiseTheSpreadsheetColumnIndexes(
            NPOI.SS.UserModel.ISheet spreadsheet,
            ApprenticeshipStandardsSpreadsheetColumnIndexModel spreadsheetColumnIndexModel,
            ApprenticeshipStandardsSpreadsheetRowModel spreadsheetRowModel)
        {
            // NameColumnIndex
            spreadsheetColumnIndexModel.NameColumnIndex = spreadsheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Name))
                .ColumnIndex;

            // ReferenceColumnIndex
            spreadsheetColumnIndexModel.ReferenceColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Reference))
                  .ColumnIndex;

            // Version_NumberColumnIndex
            spreadsheetColumnIndexModel.Version_NumberColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Version_Number).Replace('_', ' '))
                  .ColumnIndex;

            // Proposal_ApprovedColumnIndex
            spreadsheetColumnIndexModel.Proposal_ApprovedColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Proposal_Approved).Replace('_', ' '))
                  .ColumnIndex;

            // Standard_ApprovedColumnIndex
            spreadsheetColumnIndexModel.Standard_ApprovedColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Standard_Approved).Replace('_', ' '))
                  .ColumnIndex;

            // Assessment_Plan_ApprovedColumnIndex
            spreadsheetColumnIndexModel.Assessment_Plan_ApprovedColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Assessment_Plan_Approved).Replace('_', ' '))
                  .ColumnIndex;

            // StatusColumnIndex
            spreadsheetColumnIndexModel.StatusColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Status))
                  .ColumnIndex;

            // Approved_for_Delivery_DateColumnIndex
            spreadsheetColumnIndexModel.Approved_for_Delivery_DateColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Approved_for_Delivery_Date).Replace('_', ' '))
                  .ColumnIndex;

            // RouteColumnIndex
            spreadsheetColumnIndexModel.RouteColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Route))
                  .ColumnIndex;

            // LevelColumnIndex
            spreadsheetColumnIndexModel.LevelColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Level))
                  .ColumnIndex;

            // Integrated_DegreeColumnIndex
            spreadsheetColumnIndexModel.Integrated_DegreeColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Integrated_Degree).Replace('_', ' '))
                  .ColumnIndex;

            // Maximum_FundingColumnIndex
            spreadsheetColumnIndexModel.Maximum_Funding_GBPColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Maximum_Funding_GBP).Replace('_', ' ').Replace("GBP", "(£)"))
                  .ColumnIndex;

            // Typical_DurationColumnIndex
            spreadsheetColumnIndexModel.Typical_DurationColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Typical_Duration).Replace('_', ' '))
                  .ColumnIndex;

            // Core_and_OptionsColumnIndex
            spreadsheetColumnIndexModel.Core_and_OptionsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Core_and_Options).Replace("_and_", " & "))
                  .ColumnIndex;

            // OptionsColumnIndex
            spreadsheetColumnIndexModel.OptionsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Options))
                  .ColumnIndex;

            // Regulated_StandardColumnIndex
            spreadsheetColumnIndexModel.Regulated_StandardColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Regulated_Standard).Replace('_', ' '))
                  .ColumnIndex;

            // Trailblazer_ContactColumnIndex
            spreadsheetColumnIndexModel.Trailblazer_ContactColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Trailblazer_Contact).Replace('_', ' '))
                  .ColumnIndex;

            // LARS_code_for_providers_onlyColumnIndex
            spreadsheetColumnIndexModel.LARS_code_for_providers_onlyColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.LARS_code_for_providers_only).Replace('_', ' '))
                  .ColumnIndex;

            // EQA_ProviderColumnIndex
            spreadsheetColumnIndexModel.EQA_ProviderColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.EQA_Provider).Replace('_', ' '))
                  .ColumnIndex;

            // LinkColumnIndex
            spreadsheetColumnIndexModel.LinkColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Link))
                  .ColumnIndex;

            // Last_UpdatedColumnIndex
            spreadsheetColumnIndexModel.Last_UpdatedColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Last_Updated).Replace('_', ' '))
                  .ColumnIndex;
        }

        private ApprenticeshipStandardsSpreadsheetRowModel GetTheSpreadsheetRowData(
            NPOI.SS.UserModel.IRow spreadsheetRow,
            ApprenticeshipStandardsSpreadsheetColumnIndexModel spreadsheetColumnIndexModel)
        {
            ApprenticeshipStandardsSpreadsheetRowModel spreadsheetRowModel = null;

            if (spreadsheetRow != null)
            {
                spreadsheetRowModel = new ApprenticeshipStandardsSpreadsheetRowModel();

                // Name
                NPOI.SS.UserModel.ICell name = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.NameColumnIndex);
                if (name != null)
                {
                    spreadsheetRowModel.Name = name.StringCellValue;
                }

                // Reference
                NPOI.SS.UserModel.ICell reference = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ReferenceColumnIndex);
                if (reference != null)
                {
                    spreadsheetRowModel.Reference = reference.StringCellValue;
                }

                // Version_Number
                NPOI.SS.UserModel.ICell version_Number = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Version_NumberColumnIndex);
                if (version_Number != null)
                {
                    spreadsheetRowModel.Version_Number = Convert.ToByte(version_Number.NumericCellValue);
                }

                // Proposal_Approved
                NPOI.SS.UserModel.ICell proposal_Approved = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Proposal_ApprovedColumnIndex);
                if (proposal_Approved != null)
                {
                    spreadsheetRowModel.Proposal_Approved = proposal_Approved.StringCellValue;
                }

                // Standard_Approved
                NPOI.SS.UserModel.ICell standard_Approved = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Standard_ApprovedColumnIndex);
                if (standard_Approved != null)
                {
                    spreadsheetRowModel.Standard_Approved = standard_Approved.StringCellValue;
                }

                // Assessment_Plan_Approved
                NPOI.SS.UserModel.ICell assessment_Plan_Approved = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Assessment_Plan_ApprovedColumnIndex);
                if (assessment_Plan_Approved != null)
                {
                    spreadsheetRowModel.Assessment_Plan_Approved = assessment_Plan_Approved.StringCellValue;
                }

                // Status
                NPOI.SS.UserModel.ICell status = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.StatusColumnIndex);
                if (status != null)
                {
                    spreadsheetRowModel.Status = status.StringCellValue;
                }

                // Approved_for_Delivery_Date
                NPOI.SS.UserModel.ICell approved_for_Delivery_Date = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Approved_for_Delivery_DateColumnIndex);
                if (approved_for_Delivery_Date != null && approved_for_Delivery_Date.CellType == NPOI.SS.UserModel.CellType.Numeric)
                {
                    spreadsheetRowModel.Approved_for_Delivery_Date = Convert.ToDateTime(approved_for_Delivery_Date.DateCellValue);
                }

                // Route
                NPOI.SS.UserModel.ICell route = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.RouteColumnIndex);
                if (route != null)
                {
                    spreadsheetRowModel.Route = route.StringCellValue;
                }

                // Level
                NPOI.SS.UserModel.ICell level = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.LevelColumnIndex);
                if (level != null && level.CellType == NPOI.SS.UserModel.CellType.Numeric)
                {
                    spreadsheetRowModel.Level = Convert.ToByte(level.NumericCellValue);
                }

                // Integrated_Degree
                NPOI.SS.UserModel.ICell integrated_Degree = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Integrated_DegreeColumnIndex);
                if (integrated_Degree != null)
                {
                    spreadsheetRowModel.Integrated_Degree = integrated_Degree.StringCellValue;
                }

                // Typical_Duration
                NPOI.SS.UserModel.ICell typical_Duration = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Typical_DurationColumnIndex);
                if (typical_Duration != null)
                {
                    spreadsheetRowModel.Typical_Duration = Convert.ToByte(typical_Duration.NumericCellValue);
                }

                // Core_and_Options
                NPOI.SS.UserModel.ICell core_and_Options = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Core_and_OptionsColumnIndex);
                if (core_and_Options != null)
                {
                    spreadsheetRowModel.Core_and_Options = core_and_Options.StringCellValue;
                }

                // Options
                NPOI.SS.UserModel.ICell options = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.OptionsColumnIndex);
                if (options != null)
                {
                    spreadsheetRowModel.Options = options.StringCellValue;
                }

                // Regulated_Standard
                NPOI.SS.UserModel.ICell regulated_Standard = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Regulated_StandardColumnIndex);
                if (regulated_Standard != null)
                {
                    spreadsheetRowModel.Regulated_Standard = regulated_Standard.StringCellValue;
                }

                // Trailblazer_Contact
                NPOI.SS.UserModel.ICell trailblazer_Contact = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Trailblazer_ContactColumnIndex);
                if (trailblazer_Contact != null)
                {
                    spreadsheetRowModel.Trailblazer_Contact = trailblazer_Contact.StringCellValue;
                }

                // LARS_code_for_providers_only
                NPOI.SS.UserModel.ICell lars = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.LARS_code_for_providers_onlyColumnIndex);
                if (lars != null && lars.CellType == NPOI.SS.UserModel.CellType.Numeric)
                {
                    spreadsheetRowModel.LARS_code_for_providers_only = Convert.ToInt64(lars.NumericCellValue);
                }

                // EQA_Provider
                NPOI.SS.UserModel.ICell eQA_Provider = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.EQA_ProviderColumnIndex);
                if (eQA_Provider != null)
                {
                    spreadsheetRowModel.EQA_Provider = eQA_Provider.StringCellValue;
                }

                // Link
                NPOI.SS.UserModel.ICell link = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.LinkColumnIndex);
                if (link != null)
                {
                    spreadsheetRowModel.Link = link.StringCellValue;
                }

                // Approved_for_Delivery_Date
                NPOI.SS.UserModel.ICell last_Updated = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.Last_UpdatedColumnIndex);
                if (last_Updated != null && last_Updated.CellType == NPOI.SS.UserModel.CellType.Numeric)
                {
                    spreadsheetRowModel.Last_Updated = last_Updated.DateCellValue;
                }
            }

            return spreadsheetRowModel;
        }
    }
}
