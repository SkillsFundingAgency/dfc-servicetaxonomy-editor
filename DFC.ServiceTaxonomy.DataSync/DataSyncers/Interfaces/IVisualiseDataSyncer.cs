using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces
{
    public interface IVisualiseDataSyncer
    {
        Task<SubDataSync> GetVisualisationSubDataSync(string contentItemId, string dataSyncName, IContentItemVersion contentItemVersion);
    }
}
