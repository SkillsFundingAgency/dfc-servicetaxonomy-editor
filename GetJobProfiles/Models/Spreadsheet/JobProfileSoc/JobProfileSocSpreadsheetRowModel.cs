using System;

namespace GetJobProfiles.Models.Spreadsheet.JobProfileSoc
{
    public class JobProfileSocSpreadsheetRowModel
    {
        // Properties for the Sitefinity JobProfileSoc Spreadsheet on the exported Job Profile workbook

        // SystemParentId
        public string SystemParentId { get; set; }

        // IncludeInSitemap
        public bool? IncludeInSitemap { get; set; }

        // Id
        public string Id { get; set; }

        // DateCreated
        public DateTime? DateCreated { get; set; }

        // ItemDefaultUrl
        public string ItemDefaultUrl { get; set; }

        // UrlName
        public string UrlName { get; set; }

        // PublicationDate
        public DateTime? PublicationDate { get; set; }

        // ApprenticeshipStandards
        public string ApprenticeshipStandards { get; set; }

        // Description
        public string Description { get; set; }

        // SOCCode
        public string SOCCode { get; set; }

        // ONetOccupationalCode
        public string ONetOccupationalCode { get; set; }

        // ApprenticeshipFrameworks
        public string ApprenticeshipFrameworks { get; set; }
    }
}
