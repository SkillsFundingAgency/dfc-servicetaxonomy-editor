using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
//using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public interface IGraphSyncer
    {
        Task SyncToGraph(string contentType, JObject content);
    }
}
