using System;

namespace GetJobProfiles.Models.SiteFinity.UniversityRequirementModels
{
    public class UniversityRequirementWorksheetColumnIndexModel
    {
        // Index of the columns in the Sitefinity University Requirement worksheet on the exported Job Profile workbook
        public int SystemParentIdColumnIndex { get; set; }

        public int IncludeInSitemapColumnIndex { get; set; }

        public int IdColumnIndex { get; set; }

        public int DateCreatedColumnIndex { get; set; }

        public int ItemDefaultUrlColumnIndex { get; set; }

        public int UrlNameColumnIndex { get; set; }

        public int PublicationDateColumnIndex { get; set; }

        public int TitleColumnIndex { get; set; }

        public int InfoColumnIndex { get; set; }
    }
}
