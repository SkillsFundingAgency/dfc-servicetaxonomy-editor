using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;

namespace DFC.ServiceTaxonomy.GraphLookup.Handlers
{
    public class GraphLookupPartHandler : ContentPartHandler<GraphLookupPart>
    {
        public override Task InitializingAsync(InitializingContentContext context, GraphLookupPart part)
        {
            part.Show = true;

            return Task.CompletedTask;
        }
    }
}