using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class PersonalityShortQuestionContentItem : ContentItem
    {
        public PersonalityShortQuestionContentItem(string title, string timestamp, string contentItemId = null)
       : base("PersonalityShortQuestion", title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("PersonalityShortQuestion");
            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public PersonalityShortQuestionPart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
