using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.DataSync.OrchardCore.Wrappers;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts
{
    public class DataSyncOperationContext : IDataSyncOperationContext
    {
        public ContentItem ContentItem { get; }
        public IContentManager ContentManager { get; }
        public IContentItemVersion ContentItemVersion { get; protected set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentPartFieldDefinition? ContentPartFieldDefinition { get; private set; }

        public ISyncNameProvider SyncNameProvider { get; }

        protected readonly ILogger _logger;

        protected DataSyncOperationContext(
            ContentItem contentItem,
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            ILogger logger)
        {
            _logger = logger;
            ContentItem = contentItem;
            SyncNameProvider = syncNameProvider;
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
