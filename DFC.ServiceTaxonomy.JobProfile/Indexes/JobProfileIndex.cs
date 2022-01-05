using System.Collections.Generic;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.JobProfile.Indexes
{
    public class JobProfileIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? GraphSyncPartId { get; set; }
        public IList<string> DynamicTitlePrefix { get; set; }
        public IList<string> JobProfileSpecialism { get; set; }
        public IList<string> JobProfileCategory { get; set; }
        public IList<string> RelatedCareerProfiles { get; set; }
        public IList<string> SOCCode { get; set; }
        public IList<string> HiddenAlternativeTitle { get; set; }
        public IList<string> WorkingHoursDetail { get; set; }
        public IList<string> WorkingPatterns { get; set; }
        public IList<string> WorkingPatternDetail { get; set; }
        public IList<string> UniversityEntryRequirements { get; set; }
        public IList<string> UniversityRequirements { get; set; }
        public IList<string> UniversityLinks { get; set; }
        public IList<string> CollegeentryRequirements { get; set; }
        public IList<string> CollegeRequirements { get; set; }
        public IList<string> CollegeLink { get; set; }
        public IList<string> ApprenticeshipEntryRequirements { get; set; }
        public IList<string> ApprenticeshipRequirements { get; set; }
        public IList<string> ApprenticeshipLink { get; set; }
        public IList<string> Registration { get; set; }
        public IList<string> Digitalskills { get; set; }
        public IList<string> Location { get; set; }
        public IList<string> Environment { get; set; }
        public IList<string> Uniform { get; set; }
    }
}
