using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Wrappers;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class GraphOperationContext : IGraphOperationContext
    {
        public ContentItem ContentItem { get; }
        public IContentManager ContentManager { get; }
        public IContentItemVersion ContentItemVersion { get; protected set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentPartFieldDefinition? ContentPartFieldDefinition { get; private set; }

        public IGraphSyncHelper GraphSyncHelper { get; }

        protected readonly ILogger _logger;

        protected GraphOperationContext(
            ContentItem contentItem,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            ILogger logger)
        {
            _logger = logger;
            ContentItem = contentItem;
            GraphSyncHelper = graphSyncHelper;
            ContentManager = contentManager;
            ContentItemVersion = contentItemVersion;

            // will be set before any syncers receive a context
            ContentTypePartDefinition = default!;
        }

        public void SetContentPartFieldDefinition(ContentPartFieldDefinition? contentPartFieldDefinition)
        {
            ContentPartFieldDefinition = contentPartFieldDefinition != null
                ? new ContentPartFieldDefinitionWrapper(contentPartFieldDefinition) : default;
        }

        public override string ToString()
        {
            return ContentItem.ToString();
        }
    }
}
