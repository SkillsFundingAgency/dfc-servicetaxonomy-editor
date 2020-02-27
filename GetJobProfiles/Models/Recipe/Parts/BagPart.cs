using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class BagPart
    {
        public BagPart(string name = null)
        {
            //todo: serialize part with given name
        }

        public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
    }
}
