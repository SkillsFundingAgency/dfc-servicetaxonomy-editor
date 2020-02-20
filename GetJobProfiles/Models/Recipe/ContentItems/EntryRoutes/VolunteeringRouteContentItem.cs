using System.Collections.Generic;
using System.Text.Json.Serialization;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class VolunteeringRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public VolunteeringRouteContentItem(string title, string timestamp, IEnumerable<string> description) : base("VolunteeringRoute", title, timestamp, description)
        {
        }

        [JsonPropertyName("VolunteeringRoute")]
        public override TitleHtmlDescriptionPart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
