using System.Collections.Generic;
using System.Text.Json.Serialization;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class VolunteeringRouteContentItem : EntryRouteContentItem
    {
        public VolunteeringRouteContentItem(string timestamp, IEnumerable<string> description)
            : base("VolunteeringRoute", timestamp, description)
        {
        }

        [JsonPropertyName("VolunteeringRoute")]
        public override EntryRoutePart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
