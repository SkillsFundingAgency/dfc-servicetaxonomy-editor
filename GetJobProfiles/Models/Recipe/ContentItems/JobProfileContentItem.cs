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
        public JobProfilePart EponymousPart { get; set; }
        //public GraphLookupPart GraphLookupPart { get; set; }    // todo: multiple?
        public GraphSyncPart GraphSyncPart { get; set; }
        public BagPart EntryRoutes { get; set; }
    }

    public class JobProfilePart
    {
        public HtmlField Description { get; set; }
        public TextField JobProfileWebsiteUrl { get; set; }
        public ContentPicker SOCCode { get; set; }

        #region How To Become
        public HtmlField HtbBodies { get; set; }
        //todo: this field
        public TextField HtbTitleOptions { get; set; }
        // public HtmlField HtbOtherRequirements { get; set; }
        public HtmlField HtbCareerTips { get; set; }
        public HtmlField HtbFurtherInformation { get; set; }
        public ContentPicker HtbRegistrations { get; set; }
        #endregion How To Become

        #region What It Takes
        public HtmlField WitDigitalSkillsLevel { get; set; }
        public ContentPicker WitRestrictions { get; set; }
        public ContentPicker WitOtherRequirements { get; set; }
        #endregion What It Takes

        #region What You'll Do
        public ContentPicker DayToDayTasks { get; set; }
        public ContentPicker WydWorkingEnvironment { get; set; }
        public ContentPicker WydWorkingLocation { get; set; }
        public ContentPicker WydWorkingUniform { get; set; }
        #endregion What You'll Do

        public TextField SalaryStarter { get; set; }
        public TextField SalaryExperienced { get; set; }
        public NumericField MinimumHours { get; set; }
        public NumericField MaximumHours { get; set; }
        public TextField WorkingHoursDetails { get; set; }
        public TextField WorkingPattern { get; set; }
        public TextField WorkingPatternDetails { get; set; }
        public HtmlField CareerPathAndProgression { get; set; }

        public ContentPicker ApprenticeshipStandards { get; set; }
    }
}
