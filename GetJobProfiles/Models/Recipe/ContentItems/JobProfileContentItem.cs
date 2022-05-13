using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class JobProfileContentItem : ContentItem
    {
        public JobProfileContentItem(string title, string timestamp)
            : base("JobProfile", title, timestamp)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("JobProfile");

            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public PageLocationPart PageLocationPart { get; set; }
        public JobProfilePart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class JobProfilePart
    {
        public TaxonomyField PageLocations { get; set; }
        public ContentPicker Occupation { get; set; }
        public HtmlField Description { get; set; }
        public TextField TitleOptions { get; set; }
        public ContentPicker SOCCode { get; set; }
        public ContentPicker ONetOccupationalCode { get; set; }
        public TextField SalaryStarter { get; set; }
        public TextField SalaryExperienced { get; set; }
        public NumericField MinimumHours { get; set; }
        public NumericField MaximumHours { get; set; }
        public TextField WorkingHoursDetails { get; set; }
        public TextField WorkingPattern { get; set; }
        public TextField WorkingPatternDetails { get; set; }

        //How to become
        public ContentPicker ApprenticeshipRoute { get; set; }
        public ContentPicker CollegeRoute { get; set; }
        public ContentPicker UniversityRoute { get; set; }
        public ContentPicker DirectRoute { get; set; }
        public ContentPicker OtherRoute { get; set; }
        public ContentPicker VolunteeringRoute { get; set; }
        public ContentPicker WorkRoute { get; set; }
        public HtmlField HtbBodies { get; set; }
        // public HtmlField HtbOtherRequirements { get; set; }
        public HtmlField HtbCareerTips { get; set; }
        public HtmlField HtbFurtherInformation { get; set; }
        public ContentPicker HtbRegistrations { get; set; }

        //What it takes
        public HtmlField WitDigitalSkillsLevel { get; set; }
        public ContentPicker WitRestrictions { get; set; }
        public ContentPicker WitOtherRequirements { get; set; }

        //What you'll do
        public ContentPicker DayToDayTasks { get; set; }
        public ContentPicker WydWorkingEnvironment { get; set; }
        public ContentPicker WydWorkingLocation { get; set; }
        public ContentPicker WydWorkingUniform { get; set; }

        //Career path
        public HtmlField CareerPathAndProgression { get; set; }
        public ContentPicker ApprenticeshipStandards { get; set; }
    }
}
