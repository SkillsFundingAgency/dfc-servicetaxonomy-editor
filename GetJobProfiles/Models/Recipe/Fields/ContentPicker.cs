using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GetJobProfiles.Models.Recipe.Fields
{
    public class ContentPicker
    {
        public ContentPicker()
        {
        }

        //todo: this will go
        public ContentPicker(ConcurrentDictionary<string, (string id, string text)> currentContentItems, IEnumerable<string> contentItems)
        {
            ContentItemIds = contentItems?.Select(ci => currentContentItems[ci].id) ?? new string[0];
        }

        public IEnumerable<string> ContentItemIds { get; set; } = new string[0];
    }
}
