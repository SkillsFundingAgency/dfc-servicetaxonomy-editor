using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    public class GraphSyncPartHandler : ContentPartHandler<GraphSyncPart>
    {
        public override Task InitializingAsync(InitializingContentContext context, GraphSyncPart part)
        {
            part.Show = true;

            return Task.CompletedTask;
        }
    }
}