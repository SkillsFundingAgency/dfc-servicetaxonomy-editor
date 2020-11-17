using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class SlackMessagePublisher : ISlackMessagePublisher
    {
        private readonly HttpClient _client;
        private const string WebhookEndpoint = "https://hooks.slack.com/services/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";

        public SlackMessagePublisher(HttpClient client)
        {
            _client = client;
        }

        public async Task SendMessageAsync(string text)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(new { Text = text }), Encoding.UTF8, "application/json");

            await _client.PostAsync(WebhookEndpoint, content);
        }
    }
}
