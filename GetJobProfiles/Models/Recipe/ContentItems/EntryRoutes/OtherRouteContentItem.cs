using System.Collections.Generic;
using System.Text.Json.Serialization;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class OtherRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public OtherRouteContentItem(string title, string timestamp, IEnumerable<string> description) : base("OtherRoute", title, timestamp, description)
        {
        }

        [JsonPropertyName("OtherRoute")]
        public override TitleHtmlDescriptionPart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
