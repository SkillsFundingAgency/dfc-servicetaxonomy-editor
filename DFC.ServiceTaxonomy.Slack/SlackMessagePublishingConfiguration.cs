namespace DFC.ServiceTaxonomy.Slack
{
    public class SlackMessagePublishingConfiguration
    {
        public bool? PublishToSlack { get; set; }
        public string? SlackWebhookEndpoint { get; set; }
    }
}
