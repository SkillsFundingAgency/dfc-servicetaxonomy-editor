using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class OtherRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public OtherRouteContentItem(string title, string timestamp, IEnumerable<string> description,
            string contentItemId)
            : base("OtherRoute", title, timestamp, description, contentItemId)
        {
        }
    }

    // {
    //     public OtherRouteContentItem(string timestamp, IEnumerable<string> description)
    //         : base("OtherRoute", timestamp, description)
    //     {
    //     }
    //
    //     [JsonPropertyName("OtherRoute")]
    //     public override EntryRoutePart EponymousPart {
    //         get { return base.EponymousPart; }
    //         set { base.EponymousPart = value; }
    //     }
    // }
}
