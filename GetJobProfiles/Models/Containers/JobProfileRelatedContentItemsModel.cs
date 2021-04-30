using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Containers
{
    public class JobProfileRelatedContentItemsModel
    {
        // Onet skill
        // public ONetSkillContentItemBuilder {get; set;}

        // University Requirement

        // University Link
        // College Requirement
        // College Link
        // Apprencticeship Requirement
        // Apprenticeship Link

        // Apprencticeship Standard Route
        public ApprenticeshipStandardRouteContentItem ApprenticeshipStandardRouteContentItems { get; set; }

        // QCF Level
        public QcfLevelContentItem QCFLevelContentItems { get; set; }
        // SOC Code
        // ONet Occupational Code
        public ONetOccupationalCodeContentItem ONetOccupationalCodeContentItems { get; set; }

        // University Route
        // College Route
        // Apprenticeship Route
        // Work Route
        // Volunteering Route
        // Direct Route
        // Other Route
        // Registration
        // Restriction
        // Other Requirement
        public OtherRequirementContentItem OtherRequirementContentItems { get; set; }

        // Digital Skills Level
        public DigitalSkillsLevelContentItem DigitalSkillsLevelContentItems { get; set; }

        // Working Environment
        // Working Location
        // Working Uniform
        // Apprencticeship Standard
        public ApprenticeshipStandardContentItem ApprenticeshipStandardContentItems { get; set;}

        // Job Profiles
        public JobProfileContentItem JobProfileContentItems { get; set; }

        // Job Category
        public JobCategoryContentItem JobCategoryContentItems { get; set; }
    }
}
