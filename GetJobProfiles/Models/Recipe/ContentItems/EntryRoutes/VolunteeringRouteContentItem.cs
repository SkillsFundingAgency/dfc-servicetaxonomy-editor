using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class VolunteeringRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public VolunteeringRouteContentItem(string title, string timestamp, IEnumerable<string> description,
            string contentItemId)
            : base("VolunteeringRoute", title, timestamp, description, contentItemId)
        {
        }
    }
    // {
    //     public VolunteeringRouteContentItem(string timestamp, IEnumerable<string> description)
    //         : base("VolunteeringRoute", timestamp, description)
    //     {
    //     }
    //
    //     [JsonPropertyName("VolunteeringRoute")]
    //     public override EntryRoutePart EponymousPart {
    //         get { return base.EponymousPart; }
    //         set { base.EponymousPart = value; }
    //     }
    // }
}
