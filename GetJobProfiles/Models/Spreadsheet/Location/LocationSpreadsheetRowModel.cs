using System;

namespace GetJobProfiles.Models.Spreadsheet.Location
{
    public class LocationSpreadsheetRowModel
    {
        // Properties for the Sitefinity Location spreadsheet on the exported Job Profile workbook

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

        // Description
        public string Description { get; set; }

        // IsNegative
        public string IsNegative { get; set; }

        // Title
        public string Title { get; set; }

    }
}
