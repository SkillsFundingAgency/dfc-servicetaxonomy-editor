using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes
{
    public class WorkRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public WorkRouteContentItem(string title, string timestamp, IEnumerable<string> description) : base("WorkRoute", title, timestamp, description)
        {
        }
    }
}
