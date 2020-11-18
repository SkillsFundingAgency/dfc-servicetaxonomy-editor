namespace DFC.ServiceTaxonomy.GraphSync.Configuration
{
    public class SlackMessagePublishingConfiguration
    {
        public bool? PublishToSlack { get; set; }
        public string? SlackWebhookEndpoint { get; set; }
    }
}
