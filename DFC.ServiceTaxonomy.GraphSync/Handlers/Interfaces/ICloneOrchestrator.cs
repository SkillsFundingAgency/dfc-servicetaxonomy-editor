using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces
{
    public interface ICloneOrchestrator
    {
        Task<bool> Clone(ContentItem contentItem);
    }
}
