

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

        public const string Environment = nameof(Environment);
        public const string Uniform = nameof(Uniform);
        public const string Location = nameof(Location);

        public const string UniversityLink = nameof(UniversityLink);
        public const string CollegeLink = nameof(CollegeLink);
        public const string ApprenticeshipLink = nameof(ApprenticeshipLink);

        public const string Restriction = nameof(Restriction);
        public const string Registration = nameof(Registration);
        public const string ApprenticeshipRequirement = nameof(ApprenticeshipRequirement);
        public const string CollegeRequirement = nameof(CollegeRequirement);
        public const string UniversityRequirement = nameof(UniversityRequirement);

        public const string HiddenAlternativeTitle = nameof(HiddenAlternativeTitle);
        public const string JobProfileSpecialism = nameof(JobProfileSpecialism);
        public const string Workinghoursdetail = nameof(Workinghoursdetail);
        public const string Workingpatterns = nameof(Workingpatterns);
        public const string Workingpatterndetail = nameof(Workingpatterndetail);
    }
}
