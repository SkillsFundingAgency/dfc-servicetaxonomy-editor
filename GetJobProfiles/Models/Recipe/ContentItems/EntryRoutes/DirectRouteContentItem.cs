using System.Collections.Generic;
using System.Text.Json.Serialization;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class DirectRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public DirectRouteContentItem(string title, string timestamp, IEnumerable<string> description)
            : base("DirectRoute", title, timestamp, description)
        {
        }

        [JsonPropertyName("DirectRoute")]
        public override TitleHtmlDescriptionPart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
