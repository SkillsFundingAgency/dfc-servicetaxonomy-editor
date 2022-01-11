using System;
using System.Collections.Generic;
using System.Text;

namespace GetJobProfiles.Models.Recipe.Spreadsheet.Uniform
{
    public class UniformSpreadsheetRowModel
    {
        // Properties for the Sitefinity Uniform Spreadsheet on the exported Job Profile workbook

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

        // IsNegative
        public bool? IsNegative { get; set; }
    }
}
