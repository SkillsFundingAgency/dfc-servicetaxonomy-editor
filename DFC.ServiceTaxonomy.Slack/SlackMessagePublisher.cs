using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.Slack
{
    public class SlackMessagePublisher : ISlackMessagePublisher
    {
        private readonly IOptionsMonitor<SlackMessagePublishingConfiguration> _configMonitor;
        private readonly HttpClient _client;

        public SlackMessagePublisher(IOptionsMonitor<SlackMessagePublishingConfiguration> configMonitor, HttpClient client)
        {
            _client = client;
            _configMonitor = configMonitor;
        }

        public async Task SendMessageAsync(string text)
        {
            if (_configMonitor.CurrentValue.PublishToSlack ?? false)
            {
                if (string.IsNullOrWhiteSpace(_configMonitor.CurrentValue.SlackWebhookEndpoint))
                    throw new InvalidOperationException(nameof(_configMonitor.CurrentValue.SlackWebhookEndpoint));

                await SendMessageAsync(_configMonitor.CurrentValue.SlackWebhookEndpoint!, text);
            }
        }

        public async Task SendMessageAsync(string webhookEndpoint, string text)
        {
            if (_configMonitor.CurrentValue.PublishToSlack ?? false)
            {
                StringContent content = new(JsonConvert.SerializeObject(new { text }), Encoding.UTF8,
                    "application/json");

                await _client.PostAsync(webhookEndpoint, content);
            }
        }
    }
}
