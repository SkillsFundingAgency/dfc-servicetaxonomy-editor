using System;

namespace GetJobProfiles.Models.Spreadsheet.ApprenticeshipStandards
{
    public class ApprenticeshipStandardsSpreadsheetRowModel
    {
        // Properties for the Apprenticeship Standards spreadsheet

        // Name
        public string Name { get; set; }

        // Reference
        public string Reference { get; set; }

        // Version_Number
        public byte? Version_Number { get; set; }

        // Proposal_Approved
        public string Proposal_Approved { get; set; }

        // Standard_Approved
        public string Standard_Approved { get; set; }

        // Assessment_PlanApproved
        public string Assessment_Plan_Approved { get; set; }

        // Status
        public string Status { get; set; }

        // Approved_for_DeliveryDate
        public DateTime? Approved_for_Delivery_Date { get; set; }

        // Route
        public string Route { get; set; }

        // Level
        public byte? Level { get; set; }

        // Integrated_Degree
        public string Integrated_Degree { get; set; }

        // Maximum_Funding
        public double? Maximum_Funding_GBP { get; set; }

        // Typical_Duration
        public byte? Typical_Duration { get; set; }

        // Core_and_Options
        public string Core_and_Options { get; set; }

        // Options
        public string Options { get; set; }

        // Regulated_Standard
        public string Regulated_Standard { get; set; }

        // Trailblazer_Contact
        public string Trailblazer_Contact { get; set; }

        // LARS_code_for_providersOnly
        public long? LARS_code_for_providers_only { get; set; }

        // EQA_Provider
        public string EQA_Provider { get; set; }

        // Link
        public string Link { get; set; }

        // Last_Updated
        public DateTime? Last_Updated { get; set; }
    }
}
