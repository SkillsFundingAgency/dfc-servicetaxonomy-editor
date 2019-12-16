using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.GraphLookup.Handlers
{
    //todo: bulk publish doesn't sync
    public class GraphLookupPartHandler : ContentPartHandler<GraphLookupPart>
    {
        public override Task UpdatedAsync(UpdateContentContext context, GraphLookupPart part)
        {
            part.Apply();

            return Task.CompletedTask;
        }
    }
}
