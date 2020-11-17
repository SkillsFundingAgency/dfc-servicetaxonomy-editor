using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public interface ISlackMessagePublisher
    {
        Task SendMessageAsync(string text);
    }
}
