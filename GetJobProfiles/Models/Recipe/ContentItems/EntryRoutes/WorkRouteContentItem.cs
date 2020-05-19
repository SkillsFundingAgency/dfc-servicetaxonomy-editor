using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class WorkRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public WorkRouteContentItem(string title, string timestamp, IEnumerable<string> description,
            string contentItemId)
            : base("WorkRoute", title, timestamp, description, contentItemId)
        {
        }
    }
    // {
    //     public WorkRouteContentItem(string timestamp, IEnumerable<string> description)
    //         : base("WorkRoute", timestamp, description)
    //     {
    //     }
    //
    //     [JsonPropertyName("WorkRoute")]
    //     public override EntryRoutePart EponymousPart {
    //         get { return base.EponymousPart; }
    //         set { base.EponymousPart = value; }
    //     }
    // }
}
