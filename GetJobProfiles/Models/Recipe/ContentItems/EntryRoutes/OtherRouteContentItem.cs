using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class OtherRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public OtherRouteContentItem(string title, string timestamp, IEnumerable<string> description, string contentItemId = null)
            : base("OtherRoute", title, timestamp, description, contentItemId)
        {
        }
    }
}
