using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.GraphLookup.Handlers
{
    public class GraphLookupPartHandler : ContentPartHandler<GraphLookupPart>
    {
        public override Task UpdatedAsync(UpdateContentContext context, GraphLookupPart part)
        {
            part.Apply();

            return Task.CompletedTask;
        }
    }
}
