

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus.Configuration
{
    internal class ServiceBusSettings
    {
        public string? ServiceBusConnectionString { get; set; }
        public string? ServiceBusTopicName { get; set; }
        public string? ServiceBusTopicNameForDraft { get; set; }
    }
}
