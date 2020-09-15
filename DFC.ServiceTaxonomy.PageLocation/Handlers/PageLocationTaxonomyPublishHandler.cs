using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.Taxonomies.Handlers;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.PageLocation.Handlers
{
    public class PageLocationTaxonomyPublishHandler : ITaxonomyTermPublishHandler
    {
        private readonly IContentOrchestrationHandler _contentOrchestrationHandler;

        public PageLocationTaxonomyPublishHandler(IContentOrchestrationHandler contentOrchestrationHandler)
        {
            _contentOrchestrationHandler = contentOrchestrationHandler;
        }

        public async Task Publish(ContentItem contentItem)
        {
            await _contentOrchestrationHandler.Published(contentItem);
        }
    }
}
