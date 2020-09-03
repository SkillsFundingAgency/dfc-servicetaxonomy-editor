using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators
{
    public class ContentTypeOrchestrator : Orchestrator, IContentTypeOrchestrator
    {
        private readonly IGraphResyncer _graphResyncer;
        private readonly IServiceProvider _serviceProvider;

        public const string ZombieFlag = "Zombie";

        public ContentTypeOrchestrator(
            IGraphResyncer graphResyncer,
            IServiceProvider serviceProvider,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            ILogger<ContentTypeOrchestrator> logger)
            : base(contentDefinitionManager, notifier, logger)
        {
            _graphResyncer = graphResyncer;
            _serviceProvider = serviceProvider;
        }

        public async Task DeleteItemsOfType(string contentTypeName)
        {
            try
            {
                //todo: does it need to be 2 phase?
                IDeleteTypeGraphSyncer publishedDeleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteTypeGraphSyncer>();
                IDeleteTypeGraphSyncer previewDeleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteTypeGraphSyncer>();

                // delete all nodes by type
                await Task.WhenAll(
                        publishedDeleteGraphSyncer.DeleteNodesByType(GraphReplicaSetNames.Published, contentTypeName),
                        previewDeleteGraphSyncer.DeleteNodesByType(GraphReplicaSetNames.Preview, contentTypeName));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Graph resync failed after deleting the {ContentType} content type.",
                    contentTypeName);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler),
                    $"Graph resync failed after deleting the {contentTypeName} content type."));
                throw;
            }
        }

        public async Task RemovePartFromItemsOfType(string contentTypeName, string contentPartName)
        {
            try
            {
                IEnumerable<ContentTypeDefinition> contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();
                var affectedContentTypeDefinition = contentTypeDefinitions
                    .Single(t => t.Name == contentTypeName);

                var affectedContentPartDefinition = affectedContentTypeDefinition.Parts
                    .Single(pd => pd.PartDefinition.Name == contentPartName)
                    .PartDefinition;

                // the content part isn't removed until after this event,
                // so we set a flag not to sync the removed part
                affectedContentPartDefinition.Settings["ContentPartSettings"]![ZombieFlag] = true;

                await _graphResyncer.ResyncContentItems(contentTypeName);
            }
            catch (Exception e)
            {
                //todo: do we need to cancel the session?
                _logger.LogError(e, "Unable to update graphs following {ContentPart} part removal from {ContentType} type.",
                    contentPartName, contentTypeName);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler),
                    $"Unable to update graphs following {contentPartName} part removal from {contentTypeName} type."));
                throw;
            }
        }

        public async Task RemoveFieldFromItemsWithPart(string contentPartName, string contentFieldName)
        {
            try
            {
                IEnumerable<ContentTypeDefinition> contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();
                var affectedContentTypeDefinitions = contentTypeDefinitions
                    .Where(t => t.Parts
                        .Any(p => p.PartDefinition.Name == contentPartName))
                    .ToArray();

                var affectedContentTypeNames = affectedContentTypeDefinitions
                    .Select(t => t.Name);

                var affectedContentFieldDefinitions = affectedContentTypeDefinitions
                    .SelectMany(td => td.Parts)
                    .Where(pd => pd.PartDefinition.Name == contentPartName)
                    .SelectMany(pd => pd.PartDefinition.Fields)
                    .Where(fd => fd.Name == contentFieldName);

                foreach (var affectedContentFieldDefinition in affectedContentFieldDefinitions)
                {
                    // the content field definition isn't removed until after this event,
                    // so we set a flag not to sync the removed field
                    affectedContentFieldDefinition.Settings["ContentPartFieldSettings"]![ZombieFlag] = true;
                }

                foreach (string affectedContentTypeName in affectedContentTypeNames)
                {
                    await _graphResyncer.ResyncContentItems(affectedContentTypeName);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to update graphs following {ContentField} field removed from {ContentPart} part.",
                    contentFieldName, contentPartName);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler),
                    $"Unable to update graphs following {contentFieldName} field removed from {contentPartName} part."));
                throw;
            }
        }
    }
}
