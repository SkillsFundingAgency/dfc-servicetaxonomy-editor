using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class BagPart
    {
        public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
    }
}
