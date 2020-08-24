using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface ICloneGraphSync
    {
        Task MutateOnClone(ContentItem contentItem, IContentManager contentManager);
    }
}
