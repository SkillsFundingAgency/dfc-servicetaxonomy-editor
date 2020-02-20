using System.Collections.Generic;
using System.Text.Json.Serialization;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class WorkRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public WorkRouteContentItem(string title, string timestamp, IEnumerable<string> description)
            : base("WorkRoute", title, timestamp, description)
        {
        }

        [JsonPropertyName("WorkRoute")]
        public override TitleHtmlDescriptionPart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
