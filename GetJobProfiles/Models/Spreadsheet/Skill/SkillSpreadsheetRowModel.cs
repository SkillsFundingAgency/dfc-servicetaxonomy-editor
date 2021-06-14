using System;

namespace GetJobProfiles.Models.Spreadsheet.Skill
{
    public class SkillSpreadsheetRowModel
    {
        // Properties for the Sitefinity Skill Spreadsheet on the exported Job Profile workbook

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

        // PSFDescription
        public string PSFDescription { get; set; }

        // PSFHidden
        public bool? PSFHidden { get; set; }

        // Description
        public string Description { get; set; }

        // ONetElementId
        public string ONetElementId { get; set; }

        // PSFCategories
        public string PSFCategories { get; set; }

        // PSFNotApplicable
        public bool? PSFNotApplicable { get; set; }

        // PSFLabel
        public string PSFLabel { get; set; }

        // PSFOrder
        public string PSFOrder { get; set; }
    }
}
