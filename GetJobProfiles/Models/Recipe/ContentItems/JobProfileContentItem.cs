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
        public HowToBecomePart HowToBecome { get; set; }
        public WhatItTakesPart WhatItTakes { get; set; }
        public WhatYouWillDoPart WhatYouWillDo { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
        public BagPart EntryRoutes { get; set; }
    }

    public class JobProfilePart
    {
        public ContentPicker Occupation { get; set; }
        public HtmlField Description { get; set; }
        public TextField JobProfileWebsiteUrl { get; set; }
        public ContentPicker SOCCode { get; set; }
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
