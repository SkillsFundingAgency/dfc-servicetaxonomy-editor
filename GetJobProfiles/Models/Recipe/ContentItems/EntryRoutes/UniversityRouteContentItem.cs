using System.Text.Json.Serialization;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class UniversityRouteContentItem : AcademicEntryRouteContentItem
    {
        public UniversityRouteContentItem(AcademicEntryRoute entryRoute, string timestamp, string contentItemId = null)
            : base("UniversityRoute", entryRoute, timestamp, contentItemId)
        {
        }

        [JsonPropertyName("UniversityRoute")]
        public override AcademicEntryRoutePart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
