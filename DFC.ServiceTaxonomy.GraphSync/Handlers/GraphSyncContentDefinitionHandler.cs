using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.Events;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    #pragma warning disable S1186
    public class GraphSyncContentDefinitionHandler : IContentDefinitionEventHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public GraphSyncContentDefinitionHandler(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context)
        {
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context)
        {
        }

        public void ContentTypeImporting(ContentTypeImportingContext context)
        {
        }

        public void ContentTypeImported(ContentTypeImportedContext context)
        {
        }

        public void ContentPartCreated(ContentPartCreatedContext context)
        {
        }

        public void ContentPartRemoved(ContentPartRemovedContext context)
        {
        }

        public void ContentPartAttached(ContentPartAttachedContext context)
        {
        }

        public void ContentPartDetached(ContentPartDetachedContext context)
        {
        }

        public void ContentPartImporting(ContentPartImportingContext context)
        {
        }

        public void ContentPartImported(ContentPartImportedContext context)
        {
        }

        public void ContentFieldAttached(ContentFieldAttachedContext context)
        {
        }

        public void ContentFieldDetached(ContentFieldDetachedContext context)
        {
            //todo: looks like old code assumed field was deleted from eponymous part (check)
            // so wouln't work with custom parts (as used in job profiles)

            //var typeBeingUpdated = _contentDefinitionManager.GetTypeDefinition(context.ContentPartName);

        }
        #pragma warning restore S1186
    }
}
