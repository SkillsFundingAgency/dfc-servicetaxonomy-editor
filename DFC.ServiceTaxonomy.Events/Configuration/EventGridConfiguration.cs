using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Events.Configuration
{
    public class EventGridConfiguration
    {
        public List<EventGridTopicConfiguration> Topics { get; set; } = new List<EventGridTopicConfiguration>();
    }
}
