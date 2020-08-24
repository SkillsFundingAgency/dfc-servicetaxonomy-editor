using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class CloneGraphSync : ICloneGraphSync
    {
        public Task MutateOnClone(ContentItem contentItem)
        {
            return Task.CompletedTask;
        }
    }
}
