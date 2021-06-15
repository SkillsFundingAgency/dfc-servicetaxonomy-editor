using System;

namespace GetJobProfiles.Models.Spreadsheet.SocSkillsMatrix
{
    public class SocSkillsMatrixSpreadsheetRowModel
    {
        // Properties for the Sitefinity SocSkillsMatrix Spreadsheet on the exported Job Profile workbook

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

        // ONetAttributeType
        public string ONetAttributeType { get; set; }

        // Rank
        public byte? Rank { get; set; }

        // Contextualised
        public string Contextualised { get; set; }

        // Title
        public string Title { get; set; }

        // ONetRank
        public decimal? ONetRank { get; set; }

        // SocCode (parsed from the first part of the Title column before the hyphen)
        public string SocCode { get; set; }

        // Skill (parsed from the second part of the Title column after the hypen)
        public string Skill { get; set; }
    }
}
