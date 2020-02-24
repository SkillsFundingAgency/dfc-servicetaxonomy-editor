using System.Collections.Generic;
using System.Text.Json.Serialization;
using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class DirectRouteContentItem : EntryRouteContentItem
    {
        public DirectRouteContentItem(string timestamp, IEnumerable<string> description)
            : base("DirectRoute", timestamp, description)
        {
        }

        [JsonPropertyName("DirectRoute")]
        public override EntryRoutePart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
