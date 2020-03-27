using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface IValidateGraphSync
    {
        Task<bool> CheckIfContentItemSynced(ContentItem contentItem);
    }
}
