using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.DataSync.Handlers
{
    public class DataSyncPartHandler : ContentPartHandler<GraphSyncPart>
    {
        public override Task InitializingAsync(InitializingContentContext context, GraphSyncPart part)
        {
            //todo: why not in template code?
            part.Apply();

            return Task.CompletedTask;
        }
    }
}
