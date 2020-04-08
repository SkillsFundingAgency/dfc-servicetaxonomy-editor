using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface IValidateGraphSync
    {
        Task ValidateGraph();
        Task<bool> CheckIfContentItemSynced(ContentItem contentItem);
    }
}
