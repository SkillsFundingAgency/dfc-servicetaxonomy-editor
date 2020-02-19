using System.Collections.Generic;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class BagPart
    {
        //can oc support >1 bagparts in a content type?
        //todo: might need BagPartContentItem?
        public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
    }
}
