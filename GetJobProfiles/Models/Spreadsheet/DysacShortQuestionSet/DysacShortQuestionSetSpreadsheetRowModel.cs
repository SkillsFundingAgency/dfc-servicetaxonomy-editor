using System;

namespace GetJobProfiles.Models.Spreadsheet.DysacShortQuestionSet
{
    public class DysacShortQuestionSetSpreadsheetRowModel
    {
        // Properties for the ShortQuestionSet spreadsheet on the Dysac workbook

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

        // Title
        public string Title { get; set; }

        // Description
        public string Description { get; set; }
    }
}
