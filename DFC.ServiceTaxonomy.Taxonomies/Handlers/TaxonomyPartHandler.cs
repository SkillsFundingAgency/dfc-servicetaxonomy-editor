using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;

namespace DFC.ServiceTaxonomy.Taxonomies.Handlers
{
    public class TaxonomyPartHandler : ContentPartHandler<TaxonomyPart>
    {
        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, TaxonomyPart part)
        {
            return context.ForAsync<ContainedContentItemsAspect>(aspect =>
            {
                aspect.Accessors.Add((jObject) =>
                {
                    return jObject["TaxonomyPart"]["Terms"] as JsonArray;
                });

                return Task.CompletedTask;
            });
        }
    }
}
