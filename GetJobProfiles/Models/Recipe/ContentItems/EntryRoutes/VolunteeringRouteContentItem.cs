using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class VolunteeringRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public VolunteeringRouteContentItem(string title, string timestamp, IEnumerable<string> description, string contentItemId = null)
            : base("VolunteeringRoute", title, timestamp, description, contentItemId)
        {
        }
    }
}
