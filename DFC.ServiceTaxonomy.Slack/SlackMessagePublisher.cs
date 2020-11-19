using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.Slack
{
    public class SlackMessagePublisher : ISlackMessagePublisher
    {
        private readonly SlackMessagePublishingConfiguration _config;
        private readonly HttpClient _client;

        public SlackMessagePublisher(IOptions<SlackMessagePublishingConfiguration> config, HttpClient client)
        {
            _client = client;
            _config = config.Value;
        }

        public async Task SendMessageAsync(string text)
        {
            if ((_config.PublishToSlack ?? false) && !string.IsNullOrWhiteSpace(_config.SlackWebhookEndpoint))
            {
                await SendMessageAsync(_config.SlackWebhookEndpoint, text);
            }
        }

        public async Task SendMessageAsync(string webhookEndpoint, string text)
        {
            if (_config.PublishToSlack ?? false)
            {
                StringContent content = new StringContent(JsonSerializer.Serialize(new { text }), Encoding.UTF8,
                    "application/json");

                await _client.PostAsync(webhookEndpoint, content);
            }
        }
    }
}
