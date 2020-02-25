using System.Collections.Generic;
using System.Text.Json.Serialization;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class WorkRouteContentItem : EntryRouteContentItem
    {
        public WorkRouteContentItem(string timestamp, IEnumerable<string> description)
            : base("WorkRoute", timestamp, description)
        {
        }

        [JsonPropertyName("WorkRoute")]
        public override EntryRoutePart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
