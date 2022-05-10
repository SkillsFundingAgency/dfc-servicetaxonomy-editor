using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Indexes
{
    public class JobProfileIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? GraphSyncPartId { get; set; }
        public string? DynamicTitlePrefix { get; set; }
        public string? JobProfileSpecialism { get; set; }
        public string? JobProfileCategory { get; set; }
        public string? RelatedCareerProfiles { get; set; }
        public string? SOCCode { get; set; }
        public string? HiddenAlternativeTitle { get; set; }
        public string? WorkingHoursDetail { get; set; }
        public string? WorkingPatterns { get; set; }
        public string? WorkingPatternDetail { get; set; }
        public string? UniversityEntryRequirements { get; set; }
        public string? UniversityRequirements { get; set; }
        public string? UniversityLinks { get; set; }
        public string? CollegeEntryRequirements { get; set; }
        public string? CollegeRequirements { get; set; }
        public string? CollegeLink { get; set; }
        public string? ApprenticeshipEntryRequirements { get; set; }
        public string? ApprenticeshipRequirements { get; set; }
        public string? ApprenticeshipLink { get; set; }
        public string? Registration { get; set; }
        public string? DigitalSkills { get; set; }
        public string? RelatedSkills { get; set; }
        public string? Location { get; set; }
        public string? Environment { get; set; }
        public string? Uniform { get; set; }
        public string? JobProfileTitle { get; set; }
        public string? Restriction { get; set; }
    }
}
