namespace DFC.ServiceTaxonomy.GraphSync.Configuration
{
    public class SlackMessagePublishingConfiguration
    {
        public bool? PublishToSlack { get; set; } = true;
        public string? SlackWebhookEndpoint { get; set; } = "https://hooks.slack.com/services/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";
    }
}
