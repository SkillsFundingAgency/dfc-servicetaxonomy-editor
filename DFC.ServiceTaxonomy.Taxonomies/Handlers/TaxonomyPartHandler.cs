using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using Newtonsoft.Json.Linq;
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
                    return jObject["TaxonomyPart"]["Terms"] as JArray;
                });

                return Task.CompletedTask;
            });
        }
    }
}
