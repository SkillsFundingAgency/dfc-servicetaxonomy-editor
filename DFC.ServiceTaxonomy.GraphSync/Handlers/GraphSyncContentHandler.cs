using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    public class GraphSyncContentHandler : ContentHandlerBase
    {
        public override Task DraftSavedAsync(SaveDraftContentContext context)
        {
            return Task.CompletedTask;
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            return Task.CompletedTask;
        }
    }
}
