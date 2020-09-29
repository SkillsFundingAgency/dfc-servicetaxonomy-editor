using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class PersonalityQuestionSetContentItem : ContentItem
    {
        public PersonalityQuestionSetContentItem(string title, string timestamp, string contentItemId = null)
    : base("PersonalityQuestionSet", title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("PersonalityQuestionSet");
            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public PersonalityQuestionSetPart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
