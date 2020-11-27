using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Events.Configuration
{
    public class EventGridConfiguration
    {
        /// <summary>
        /// Used effectively as a global publish events or not feature toggle
        /// </summary>
        public bool PublishEvents { get; set; }
        public List<EventGridTopicConfiguration> Topics { get; set; } = new();
    }
}
