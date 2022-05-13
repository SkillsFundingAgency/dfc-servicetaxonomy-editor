using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class ONetOccupationalCodeContentItem : ContentItem
    {
        private const string CONTENT_TYPE = "ONetOccupationalCode";

        public ONetOccupationalCodeContentItem(string title, string timestamp, string contentItemId = null)
            : base(CONTENT_TYPE, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart(CONTENT_TYPE);
            EponymousPart = new ONetOccupationalCodePart();
        }

        public TitlePart TitlePart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
        public virtual ONetOccupationalCodePart EponymousPart { get; set; }
    }
}
