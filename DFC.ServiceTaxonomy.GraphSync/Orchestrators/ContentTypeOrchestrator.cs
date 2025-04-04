﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Handlers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Orchestrators
{
    public class ContentTypeOrchestrator : IContentTypeOrchestrator
    {
        private readonly IGraphResyncer _graphResyncer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IGraphSyncNotifier _notifier;
        private readonly IOptionsMonitor<GraphSyncPartSettingsConfiguration> _graphSyncPartSettings;
        private readonly ILogger<ContentTypeOrchestrator> _logger;

        public const string ZombieFlag = "Zombie";

        public ContentTypeOrchestrator(
            IGraphResyncer graphResyncer,
            IServiceProvider serviceProvider,
            IContentDefinitionManager contentDefinitionManager,
            IGraphSyncNotifier notifier,
            IOptionsMonitor<GraphSyncPartSettingsConfiguration> graphSyncPartSettings,
            ILogger<ContentTypeOrchestrator> logger)
        {
            _graphResyncer = graphResyncer;
            _serviceProvider = serviceProvider;
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _graphSyncPartSettings = graphSyncPartSettings;
            _logger = logger;
        }

        public void SetDefaultsForGraphSyncPart(string contentTypeName)
        {
            try
            {
                var settings = _graphSyncPartSettings.CurrentValue.Settings.Single(settingBlock => settingBlock.Name == "NCS");

                _contentDefinitionManager.AlterTypeDefinitionAsync(contentTypeName, builder => builder
                    .WithPart(nameof(GraphSyncPart), part => part
                        .WithSettings(new GraphSyncPartSettings
                        {
                            BagPartContentItemRelationshipType = settings.BagPartContentItemRelationshipType,
                            PreexistingNode = settings.PreexistingNode,
                            DisplayId = settings.DisplayId,
                            NodeNameTransform = settings.NodeNameTransform,
                            PropertyNameTransform = settings.PropertyNameTransform,
                            CreateRelationshipType = settings.CreateRelationshipType,
                            IdPropertyName = settings.IdPropertyName,
                            GenerateIdPropertyValue = settings.GenerateIdPropertyValue,
                            PreExistingNodeUriPrefix = settings.PreExistingNodeUriPrefix,
                            VisualiserNodeDepth = settings.VisualiserNodeDepth,
                            VisualiserIncomingRelationshipsPathLength = settings.VisualiserIncomingRelationshipsPathLength
                        })));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed setting data synchronisation defaults for {ContentType} content type.",
                    contentTypeName);
                _notifier.AddAsync(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler),
                    $"Failed setting data synchronisation defaults for {contentTypeName} content type.")).GetAwaiter().GetResult();

                throw;
            }
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
                await _notifier.AddAsync(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler),
                    $"Graph resync failed after deleting the {contentTypeName} content type."));
                throw;
            }
        }

        public async Task RemovePartFromItemsOfType(string contentTypeName, string contentPartName)
        {
            try
            {
                IEnumerable<ContentTypeDefinition> contentTypeDefinitions = await _contentDefinitionManager.ListTypeDefinitionsAsync();
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
                await _notifier.AddAsync(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler),
                    $"Unable to update graphs following {contentPartName} part removal from {contentTypeName} type."));
                throw;
            }
        }

        public async Task RemoveFieldFromItemsWithPart(string contentPartName, string contentFieldName)
        {
            try
            {
                IEnumerable<ContentTypeDefinition> contentTypeDefinitions = await _contentDefinitionManager.ListTypeDefinitionsAsync();
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
                await _notifier.AddAsync(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentDefinitionHandler),
                    $"Unable to update graphs following {contentFieldName} field removed from {contentPartName} part."));
                throw;
            }
        }
    }
}
