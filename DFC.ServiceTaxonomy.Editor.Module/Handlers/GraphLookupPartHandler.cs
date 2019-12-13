using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Parts;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.Editor.Module.Handlers
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
