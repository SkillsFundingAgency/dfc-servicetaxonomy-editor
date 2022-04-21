using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Handlers;
using DFC.ServiceTaxonomy.DataSync.Notifications;
using DFC.ServiceTaxonomy.DataSync.Orchestrators.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.DataSync.Orchestrators
{
    public class ContentTypeOrchestrator : IContentTypeOrchestrator
    {
        private readonly IDataResyncer _dataResyncer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IDataSyncNotifier _notifier;
        private readonly ILogger<ContentTypeOrchestrator> _logger;

        public const string ZombieFlag = "Zombie";

        public ContentTypeOrchestrator(
            IDataResyncer dataResyncer,
            IServiceProvider serviceProvider,
            IContentDefinitionManager contentDefinitionManager,
            IDataSyncNotifier notifier,
            ILogger<ContentTypeOrchestrator> logger)
        {
            _dataResyncer = dataResyncer;
            _serviceProvider = serviceProvider;
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _logger = logger;
        }

        public async Task DeleteItemsOfType(string contentTypeName)
        {
            try
            {
                //todo: does it need to be 2 phase?
                IDeleteTypeDataSyncer publishedDeleteDataSyncer = _serviceProvider.GetRequiredService<IDeleteTypeDataSyncer>();
                IDeleteTypeDataSyncer previewDeleteDataSyncer = _serviceProvider.GetRequiredService<IDeleteTypeDataSyncer>();

                // delete all nodes by type
                await Task.WhenAll(
                        publishedDeleteDataSyncer.DeleteNodesByType(DataSyncReplicaSetNames.Published, contentTypeName),
                        previewDeleteDataSyncer.DeleteNodesByType(DataSyncReplicaSetNames.Preview, contentTypeName));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DataSync resync failed after deleting the {ContentType} content type.",
                    contentTypeName);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DataSyncContentDefinitionHandler),
                    $"DataSync resync failed after deleting the {contentTypeName} content type."));
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

                await _dataResyncer.ResyncContentItems(contentTypeName);
            }
            catch (Exception e)
            {
                //todo: do we need to cancel the session?
                _logger.LogError(e, "Unable to update graphs following {ContentPart} part removal from {ContentType} type.",
                    contentPartName, contentTypeName);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DataSyncContentDefinitionHandler),
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
                    await _dataResyncer.ResyncContentItems(affectedContentTypeName);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to update graphs following {ContentField} field removed from {ContentPart} part.",
                    contentFieldName, contentPartName);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DataSyncContentDefinitionHandler),
                    $"Unable to update graphs following {contentFieldName} field removed from {contentPartName} part."));
                throw;
            }
        }
    }
}
