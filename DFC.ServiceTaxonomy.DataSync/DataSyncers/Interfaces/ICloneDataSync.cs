using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces
{
    public interface ICloneDataSync
    {
        Task MutateOnClone(
            ContentItem contentItem,
            IContentManager contentManager,
            ICloneContext? parentContext = null);
    }
}
