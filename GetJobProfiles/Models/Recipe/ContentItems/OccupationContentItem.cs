using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class OccupationContentItem : ContentItem
    {
        public OccupationContentItem(string title, string timestamp, string contentItemId = null)
            : base("Occupation", title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("Occupation");

            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        //public OccupationPart EponymousPart { get; set; }
        public GraphLookupPart GraphLookupPart { get; set; }
        //todo: need to set graph sync's Text to have same guid as esco occupation
        // ^^ add to cypher recipe after creating nodes for labels & creating relationships
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
