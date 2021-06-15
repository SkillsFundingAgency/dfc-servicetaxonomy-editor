using System;
using System.Collections.Generic;
using System.Text;

namespace GetJobProfiles.Models.Spreadsheet.ApprenticeshipLink
{
    public class ApprenticeshipLinkSpreadsheetRowModel
    {
        // Properties for the Sitefinity ApprenticeshipLink spreadsheet on the exported Job Profile workbook

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
