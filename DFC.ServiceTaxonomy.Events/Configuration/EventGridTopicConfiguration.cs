namespace DFC.ServiceTaxonomy.Events.Configuration
{
    public class EventGridTopicConfiguration
    {
        public string? ContentType { get; set; }
        public string? TopicEndpoint { get; set; }
        public string? AegSasKey { get; set; }

        //todo?
        // public string? FailoverTopicEndpoint { get; set; }
        // public string? FailoverAegSasKey { get; set; }
    }
}
