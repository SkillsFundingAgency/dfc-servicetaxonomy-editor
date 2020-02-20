using System.Text.Json.Serialization;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class CollegeRouteContentItem : AcademicEntryRouteContentItem
    {
        public CollegeRouteContentItem(AcademicEntryRoute entryRoute, string timestamp, string contentItemId = null)
            : base("CollegeRoute", entryRoute, timestamp, contentItemId)
        {
        }

        [JsonPropertyName("CollegeRoute")]
        public override AcademicEntryRoutePart EponymousPart {
            get { return base.EponymousPart; }
            set { base.EponymousPart = value; }
        }
    }
}
