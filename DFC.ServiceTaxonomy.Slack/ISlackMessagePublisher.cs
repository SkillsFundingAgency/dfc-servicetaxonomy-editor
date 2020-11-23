using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Slack
{
    public interface ISlackMessagePublisher
    {
        Task SendMessageAsync(string text);
        Task SendMessageAsync(string webhookEndpoint, string text);
    }
}
