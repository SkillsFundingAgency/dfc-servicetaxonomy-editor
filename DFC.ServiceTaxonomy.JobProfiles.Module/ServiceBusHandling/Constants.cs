namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling
{
    public static class ActionTypes
    {
        public const string Published = nameof(Published);
        public const string Deleted = nameof(Deleted);
        public const string Draft = nameof(Draft);
    }

    public static class ContentTypes
    {
        public const string JobProfile = nameof(JobProfile);
        public const string Skill = nameof(Skill);
        public const string JobProfileSOC = nameof(JobProfileSOC);
        public const string SOCSkillsMatrix = nameof(SOCSkillsMatrix);
        public const string SOCCode = nameof(SOCCode);

        public const string Environment = nameof(Environment);
        public const string Uniform = nameof(Uniform);
        public const string Location = nameof(Location);

        public const string Restriction = nameof(Restriction);
        public const string Registration = nameof(Registration);

        public const string HiddenAlternativeTitle = nameof(HiddenAlternativeTitle);
        public const string JobProfileSpecialism = nameof(JobProfileSpecialism);
        public const string WorkingHoursDetail = nameof(WorkingHoursDetail);
        public const string WorkingPatterns = nameof(WorkingPatterns);
        public const string WorkingPatternDetail = nameof(WorkingPatternDetail);

        public const string UniversityLink = nameof(UniversityLink);
        public const string CollegeLink = nameof(CollegeLink);
        public const string ApprenticeshipLink = nameof(ApprenticeshipLink);

        public const string UniversityRequirements = nameof(UniversityRequirements);
        public const string UniversityEntryRequirements = nameof(UniversityEntryRequirements);
        public const string CollegeRequirements = nameof(CollegeRequirements);
        public const string CollegeEntryRequirements = nameof(CollegeEntryRequirements);
        public const string ApprenticeshipRequirements = nameof(ApprenticeshipRequirements);
        public const string ApprenticeshipEntryRequirements = nameof(ApprenticeshipEntryRequirements);

    }
}
