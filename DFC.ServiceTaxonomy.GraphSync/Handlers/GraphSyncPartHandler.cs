using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    public class GraphSyncPartHandler : ContentPartHandler<GraphSyncPart>
    {
        public override Task InitializingAsync(InitializingContentContext context, GraphSyncPart part)
        {
            //todo: why not in template code?
            part.Apply();

            return Task.CompletedTask;
        }
    }
}
