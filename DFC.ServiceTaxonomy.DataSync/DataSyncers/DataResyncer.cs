using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers
{
    public class DataResyncer : IDataResyncer
    {
        private readonly IContentItemsService _contentItemsService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataSyncCluster _dataSyncCluster;
        private readonly IContentManager _contentManager;

        public DataResyncer(
            IContentItemsService contentItemsService,
            IServiceProvider serviceProvider,
            IDataSyncCluster dataSyncCluster,
            IContentManager contentManager)
        {
            _contentItemsService = contentItemsService;
            _serviceProvider = serviceProvider;
            _dataSyncCluster = dataSyncCluster;
            _contentManager = contentManager;
        }

        public async Task ResyncContentItems(string contentType)
        {
            var publishedDataSyncReplicaSet = _dataSyncCluster.GetDataSyncReplicaSet(DataSyncReplicaSetNames.Published);
            var previewDataSyncReplicaSet = _dataSyncCluster.GetDataSyncReplicaSet(DataSyncReplicaSetNames.Preview);

            var contentItems = await _contentItemsService.GetPublishedOnly(contentType);
            await Sync(contentItems, publishedDataSyncReplicaSet);
            await Sync(contentItems, previewDataSyncReplicaSet);

            contentItems = await _contentItemsService.GetPublishedWithDraftVersion(contentType);
            await Sync(contentItems, publishedDataSyncReplicaSet);

            contentItems = await _contentItemsService.GetDraft(contentType);
            await Sync(contentItems, previewDataSyncReplicaSet);
        }

        private async Task Sync(IEnumerable<ContentItem> contentItems, IDataSyncReplicaSet dataSyncReplicaSet)
        {
            foreach (ContentItem contentItem in contentItems)
            {
                IMergeDataSyncer mergeDataSyncer = _serviceProvider.GetRequiredService<IMergeDataSyncer>();

                //todo: we need to better handle disallowed and failed syncs
                // can we cancel the part/field detachment?
                //todo: support many items being updated in a transaction
                await mergeDataSyncer.SyncToDataSyncReplicaSetIfAllowed(dataSyncReplicaSet, contentItem, _contentManager);
            }
        }
    }
}
