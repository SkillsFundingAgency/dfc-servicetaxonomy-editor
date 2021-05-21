using System;

namespace GetJobProfiles.Models.SiteFinity.ApprencticeshipRequirementModels
{
    public class ApprenticeshipRequirementWorksheetRowModel
    {
        // Properties for the Sitefinity Apprenticeship Requirement worksheet on the exported Job Profile workbook
        public string SystemParentId { get; set; }

        public bool IncludeInSitemap { get; set; }

        public string Id { get; set; }

        public DateTime DateCreated { get; set; }

        public string ItemDefaultUrl { get; set; }

        public string UrlName { get; set; }

        public DateTime PublicationDate { get; set; }

        public string Title { get; set; }

        public string Info { get; set; }
    }
}
