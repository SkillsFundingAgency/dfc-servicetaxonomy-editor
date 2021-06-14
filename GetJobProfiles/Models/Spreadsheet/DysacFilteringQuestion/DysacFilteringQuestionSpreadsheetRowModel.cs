using System;

namespace GetJobProfiles.Models.Spreadsheet.DysacFilteringQuestion
{
    public class DysacFilteringQuestionSpreadsheetRowModel
    {
        // Properties for the FilteringQuestion spreadsheet on the Dysac workbook

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

        // DescriptionColumnIndex
        public string Description { get; set; }

        // QuestionTextColumnIndex
        public string QuestionText { get; set; }

        // TitleColumnIndex
        public string Title { get; set; }
    }
}

