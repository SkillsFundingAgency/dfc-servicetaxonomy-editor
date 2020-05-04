using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Editor.Configuration
{
    public class EventGridConfiguration
    {
        public List<EventGridTopicConfiguration> Topics { get; set; } = new List<EventGridTopicConfiguration>();
    }
}
