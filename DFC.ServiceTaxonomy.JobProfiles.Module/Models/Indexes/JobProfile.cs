using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.Indexes
{
    public class JobProfile : ContentPart
    {
        public IList<string>? DynamicTitlePrefix { get; set; }
        public IList<string>? JobProfileSpecialism { get; set; }
        public IList<string>? JobProfileCategory { get; set; }
        public IList<string>? Relatedcareerprofiles { get; set; }
        public IList<string>? SOCCode { get; set; }
        public IList<string>? HiddenAlternativeTitle { get; set; }
        public IList<string>? WorkingHoursDetails { get; set; }
        public IList<string>? WorkingPattern { get; set; }
        public IList<string>? WorkingPatternDetails { get; set; }
        public IList<string>? UniversityEntryRequirements { get; set; }
        public IList<string>? RelatedUniversityRequirements { get; set; }
        public IList<string>? RelatedUniversityLinks { get; set; }
        public IList<string>? CollegeEntryRequirements { get; set; }
        public IList<string>? RelatedCollegeRequirements { get; set; }
        public IList<string>? RelatedCollegeLinks { get; set; }
        public IList<string>? ApprenticeshipEntryRequirements { get; set; }
        public IList<string>? RelatedApprenticeshipRequirements { get; set; }
        public IList<string>? RelatedApprenticeshipLinks { get; set; }
        public IList<string>? RelatedRegistrations { get; set; }
        public IList<string>? DigitalSkills { get; set; }
        public IList<string>? RelatedLocations { get; set; }
        public IList<string>? RelatedEnvironments { get; set; }
        public IList<string>? RelatedUniforms { get; set; }
        public IList<string>? Restrictions { get; set; }
    }
}
