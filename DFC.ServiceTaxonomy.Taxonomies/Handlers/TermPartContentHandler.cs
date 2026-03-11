using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;

namespace DFC.ServiceTaxonomy.Taxonomies.Handlers
{
    public class TermPartContentHandler : ContentHandlerBase
    {
        public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            return context.ForAsync<ContainedContentItemsAspect>(aspect =>
            {
                // Check this content item contains Terms.
                if (context.ContentItem.Content.Terms is JsonArray children)
                {
                    aspect.Accessors.Add((jObject) =>
                    {
                        return jObject["Terms"] as JsonArray;
                    });
                }

                return Task.CompletedTask;
            });
        }
    }
}
