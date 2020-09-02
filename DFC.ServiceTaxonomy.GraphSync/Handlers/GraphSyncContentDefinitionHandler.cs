using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Events;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    #pragma warning disable S1186
    public class GraphSyncContentDefinitionHandler : IContentDefinitionEventHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGraphResyncer _graphResyncer;
        private readonly INotifier _notifier;
        private readonly ILogger<GraphSyncContentDefinitionHandler> _logger;

        public const string ZombieFlag = "Zombie";

        public GraphSyncContentDefinitionHandler(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider,
            IGraphResyncer graphResyncer,
            INotifier notifier,
            ILogger<GraphSyncContentDefinitionHandler> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _serviceProvider = serviceProvider;
            _graphResyncer = graphResyncer;
            _notifier = notifier;
            _logger = logger;
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context)
        {
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context)
        {
            try
            {
                //todo: does it need to be 2 phase?
                IDeleteTypeGraphSyncer publishedDeleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteTypeGraphSyncer>();
                IDeleteTypeGraphSyncer previewDeleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteTypeGraphSyncer>();

                // delete all nodes by type
                Task.WhenAll(
                    publishedDeleteGraphSyncer.DeleteNodesByType(GraphReplicaSetNames.Published, context.ContentTypeDefinition.Name),
                    previewDeleteGraphSyncer.DeleteNodesByType(GraphReplicaSetNames.Preview, context.ContentTypeDefinition.Name))
                    .GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Graph resync failed after deleting the {ContentType} content type.",
                    context.ContentTypeDefinition.Name);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler),
                    $"Graph resync failed after deleting the {context.ContentTypeDefinition.Name} content type."));
                throw;
            }
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

        //todo: this isn't getting called. is the field removed below failing? or oc issue?
        //todo: will need to delete embedded items of part, so will need to handle
        // add PartRemoved to part syncer? and call instead of add components - most defaulting to noop
        //todo: if graph sync part removed, remove all items???
        public void ContentPartRemoved(ContentPartRemovedContext context)
        {
            try
            {
                IEnumerable<ContentTypeDefinition> contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();
                var affectedContentTypeDefinitions = contentTypeDefinitions
                    .Where(t => t.Parts
                        .Any(p => p.PartDefinition.Name == context.ContentPartDefinition.Name))
                    .ToArray();

                var affectedContentTypeNames = affectedContentTypeDefinitions
                    .Select(t => t.Name);

                var affectedContentPartDefinitions = affectedContentTypeDefinitions
                    .SelectMany(td => td.Parts)
                    .Where(pd => pd.PartDefinition.Name == context.ContentPartDefinition.Name);

                foreach (var affectedContentPartDefinition in affectedContentPartDefinitions)
                {
                    // the content part isn't removed until after this event,
                    // so we set a flag not to sync the removed part
                    affectedContentPartDefinition.Settings["ContentPartSettings"]![ZombieFlag] = true;
                }

                foreach (string affectedContentTypeName in affectedContentTypeNames)
                {
                    _graphResyncer.ResyncContentItems(affectedContentTypeName).GetAwaiter().GetResult();
                }

            }
            catch (Exception e)
            {
                //todo: do we need to cancel the session?
                _logger.LogError(e, "Unable to update graphs following {ContentPart} part removal.",
                    context.ContentPartDefinition.Name);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler),
                    $"Unable to update graphs following {context.ContentPartDefinition.Name} part removal."));
                throw;
            }
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
            try
            {
                IEnumerable<ContentTypeDefinition> contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();
                var affectedContentTypeDefinitions = contentTypeDefinitions
                    .Where(t => t.Parts
                        .Any(p => p.PartDefinition.Name == context.ContentPartName))
                    .ToArray();

                var affectedContentTypeNames = affectedContentTypeDefinitions
                    .Select(t => t.Name);

                var affectedContentFieldDefinitions = affectedContentTypeDefinitions
                    .SelectMany(td => td.Parts)
                    .Where(pd => pd.PartDefinition.Name == context.ContentPartName)
                    .SelectMany(pd => pd.PartDefinition.Fields)
                    .Where(fd => fd.Name == context.ContentFieldName);

                foreach (var affectedContentFieldDefinition in affectedContentFieldDefinitions)
                {
                    // the content field definition isn't removed until after this event,
                    // so we set a flag not to sync the removed field
                    affectedContentFieldDefinition.Settings["ContentPartFieldSettings"]![ZombieFlag] = true;
                }

                foreach (string affectedContentTypeName in affectedContentTypeNames)
                {
                    _graphResyncer.ResyncContentItems(affectedContentTypeName).GetAwaiter().GetResult();
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to update graphs following {ContentField} field removed from {ContentPart} part.",
                    context.ContentFieldName, context.ContentPartName);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler),
                    $"Unable to update graphs following {context.ContentFieldName} field removed from {context.ContentPartName} part."));
                throw;
            }
        }
        #pragma warning restore S1186
    }
}
