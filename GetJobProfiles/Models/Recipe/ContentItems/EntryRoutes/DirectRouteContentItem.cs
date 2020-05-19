using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class DirectRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public DirectRouteContentItem(string title, string timestamp, IEnumerable<string> description, string contentItemId)
            : base("DirectRoute", title, timestamp, description, contentItemId)
        {
        }

        // public DirectRouteContentItem(string timestamp, IEnumerable<string> description)
        //     : base("DirectRoute", timestamp, description)
        // {
        // }
        //
        // [JsonPropertyName("DirectRoute")]
        // public override EntryRoutePart EponymousPart {
        //     get { return base.EponymousPart; }
        //     set { base.EponymousPart = value; }
        // }
    }
}
