using System.Collections.Generic;
using System.Text.Json.Serialization;
using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class OtherRouteContentItem : EntryRouteContentItem
    {
        public OtherRouteContentItem(string timestamp, IEnumerable<string> description)
            : base("OtherRoute", timestamp, description)
        {
        }

        [JsonPropertyName("OtherRoute")]
        public override EntryRoutePart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
