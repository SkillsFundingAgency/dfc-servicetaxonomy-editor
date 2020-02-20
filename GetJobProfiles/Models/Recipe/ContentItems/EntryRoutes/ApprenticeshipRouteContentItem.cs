using System.Text.Json.Serialization;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class ApprenticeshipRouteContentItem : AcademicEntryRouteContentItem
    {
        public ApprenticeshipRouteContentItem(AcademicEntryRoute entryRoute, string title, string timestamp, string contentItemId = null)
            : base("ApprenticeshipRoute", entryRoute, title, timestamp, contentItemId)
        {
        }

        [JsonPropertyName("ApprenticeshipRoute")]
        public override AcademicEntryRoutePart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
