using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class DirectRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public DirectRouteContentItem(string title, string timestamp, IEnumerable<string> description) : base("DirectRoute", title, timestamp, description)
        {
        }
    }
}
