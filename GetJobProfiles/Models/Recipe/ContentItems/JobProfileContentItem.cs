using GetJobProfiles.Models.Recipe.ContentItems.Base;
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
        public JobProfileHeaderPart JobProfileHeader { get; set; }
        public HowToBecomePart HowToBecome { get; set; }
        public WhatItTakesPart WhatItTakes { get; set; }
        public WhatYouWillDoPart WhatYouWillDo { get; set; }
        public CareerPathPart CareerPath { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class JobProfilePart
    {}
}
