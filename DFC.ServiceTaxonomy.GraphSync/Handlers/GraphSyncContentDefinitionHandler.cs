using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentTypes.Events;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    #pragma warning disable S1186
    public class GraphSyncContentDefinitionHandler : IContentDefinitionEventHandler
    {
        private readonly IContentTypeOrchestrator _contentTypeOrchestrator;
        private readonly ILogger<GraphSyncContentDefinitionHandler> _logger;

        public GraphSyncContentDefinitionHandler(
            IContentTypeOrchestrator contentTypeOrchestrator,
            ILogger<GraphSyncContentDefinitionHandler> logger)
        {
            _contentTypeOrchestrator = contentTypeOrchestrator;
            _logger = logger;
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context)
        {
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context)
        {
            _logger.LogInformation("User wants to delete content type {ContentType}.",
                context.ContentTypeDefinition.Name);

            _contentTypeOrchestrator.DeleteItemsOfType(context.ContentTypeDefinition.Name)
                .GetAwaiter().GetResult();
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

        //todo: will need to delete embedded items of part, so will need to handle
        // add PartRemoved to part syncer? and call instead of add components - most defaulting to noop
        // and removing any relationships
        public void ContentPartDetached(ContentPartDetachedContext context)
        {
            _logger.LogInformation("User wants to remove {ContentPart} from {ContentType}.",
                context.ContentPartName, context.ContentTypeName);

            if (context.ContentPartName == nameof(GraphSyncPart))
            {
                // if the graph sync part is removed that means we should remove all items of that type from the graphs
                //todo: user gets a confirmation dialog, but would be better if we double checked, informing them of the consequences!

                _logger.LogInformation("Removing all items of {ContentType} as it no longer has a GraphSyncPart.",
                    context.ContentTypeName);

                _contentTypeOrchestrator.DeleteItemsOfType(context.ContentTypeName).GetAwaiter().GetResult();
                return;
            }

            _contentTypeOrchestrator.RemovePartFromItemsOfType(context.ContentTypeName, context.ContentPartName)
                .GetAwaiter().GetResult();
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
            _logger.LogInformation("User wants to remove {ContentField} from {ContentPart}.",
                context.ContentFieldName, context.ContentPartName);

            _contentTypeOrchestrator.RemoveFieldFromItemsWithPart(context.ContentPartName, context.ContentFieldName)
                .GetAwaiter().GetResult();
        }
    }
#pragma warning disable S1186
}
