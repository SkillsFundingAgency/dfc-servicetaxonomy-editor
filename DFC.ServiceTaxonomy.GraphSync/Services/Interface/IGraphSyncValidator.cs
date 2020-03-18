using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface IGraphSyncValidator
    {
        Task<bool> CheckIfContentItemSynced(ContentItem contentItem);
    }
}
