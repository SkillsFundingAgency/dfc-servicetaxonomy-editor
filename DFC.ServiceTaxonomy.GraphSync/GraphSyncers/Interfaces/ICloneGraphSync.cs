using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface ICloneGraphSync
    {
        Task MutateOnClone(
            ContentItem contentItem,
            IContentManager contentManager,
            ICloneContext? parentContext = null);
    }
}
