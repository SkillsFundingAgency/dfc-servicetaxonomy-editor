using System;

namespace GetJobProfiles.Models.Spreadsheet.CollegeLink
{
    public class CollegeLinkSpreadsheetRowModel
    {
        // Properties for the Sitefinity CollegeLink worksheet on the exported Job Profile workbook

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

        // Url
        public string Url { get; set; }

        // Title
        public string Title { get; set; }

        // Text
        public string Text { get; set; }
    }
}
