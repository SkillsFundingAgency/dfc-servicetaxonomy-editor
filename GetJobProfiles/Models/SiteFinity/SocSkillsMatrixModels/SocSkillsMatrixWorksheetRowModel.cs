using System;

namespace GetJobProfiles.Models.SiteFinity.SocSkillsMatrixModels
{
    public class SocSkillsMatrixWorksheetRowModel
    {
        // Properties for the Sitefinity SocSkillsMatrix worksheet on the exported Job Profile workbook

        // 
        public string SystemParentId { get; set; }

        // IncludeInSitemap
        public bool IncludeInSitemap { get; set; }

        // Id
        public string Id { get; set; }

        // DateCreated
        public DateTime DateCreated { get; set; }

        // ItemDefaultUrl
        public string ItemDefaultUrl { get; set; }

        // UrlName
        public string UrlName { get; set; }

        // PublicationDate
        public DateTime PublicationDate { get; set; }

        // Title
        public string Title { get; set; }

        // ONetAttributeType
        public string ONetAttributeType { get; set; }

        // Rank
        public byte Rank { get; set; }
    }
}
