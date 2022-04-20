using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Notifications;
using DFC.ServiceTaxonomy.DataSync.Orchestrators;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Orchestrators
{
    public class SyncOrchestratorTestsBase : IDisposable
    {
        public SyncOrchestrator SyncOrchestrator { get; set; }
        public IContentDefinitionManager ContentDefinitionManager { get; set; }
        public IDataSyncNotifier Notifier { get; set; }
        public IDataSyncCluster DataSyncCluster { get; set; }
        public IPublishedContentItemVersion PublishedContentItemVersion { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public ILogger<SyncOrchestrator> Logger { get; set; }

        public ContentItem ContentItem { get; set; }
        public IMergeDataSyncer PreviewMergeDataSyncer { get; set; }
        public IMergeDataSyncer PublishedMergeDataSyncer { get; set; }
        public IAllowSync PreviewAllowSync { get; set; }
        public IAllowSync PublishedAllowSync { get; set; }
        public IContentManager ContentManager { get; set; }
        public IDataSyncReplicaSet PreviewDataSyncReplicaSet { get; set; }
        public IDataSyncReplicaSet PublishedDataSyncReplicaSet { get; set; }
        public IContentOrchestrationHandler EventGridPublishingHandler { get; set; }
        public Activity TestActivity { get; set; }

        public SyncOrchestratorTestsBase()
        {
            ContentDefinitionManager = A.Fake<IContentDefinitionManager>();
            Notifier = A.Fake<DataSyncNotifier>();

            PreviewDataSyncReplicaSet = A.Fake<IDataSyncReplicaSet>();
            A.CallTo(() => PreviewDataSyncReplicaSet.Name)
                .Returns(DataSyncReplicaSetNames.Preview);

            PublishedDataSyncReplicaSet = A.Fake<IDataSyncReplicaSet>();
            A.CallTo(() => PublishedDataSyncReplicaSet.Name)
                .Returns(DataSyncReplicaSetNames.Published);

            var graphReplicaSets = new Dictionary<string, IDataSyncReplicaSet>
            {
                { DataSyncReplicaSetNames.Preview, PreviewDataSyncReplicaSet },
                { DataSyncReplicaSetNames.Published, PublishedDataSyncReplicaSet },
            };

            DataSyncCluster = A.Fake<IDataSyncCluster>();
            A.CallTo(() => DataSyncCluster.GetDataSyncReplicaSet(A<string>._))
                .ReturnsLazily<IDataSyncReplicaSet, string>(graphReplicaSetName => graphReplicaSets[graphReplicaSetName]);

            PublishedContentItemVersion = A.Fake<IPublishedContentItemVersion>();
            A.CallTo(() => PublishedContentItemVersion.DataSyncReplicaSetName)
                .Returns(DataSyncReplicaSetNames.Published);

            ServiceProvider = A.Fake<IServiceProvider>();
            Logger = A.Fake<ILogger<SyncOrchestrator>>();

            ContentItem = new ContentItem();

            ContentManager = A.Fake<IContentManager>();
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == nameof(IContentManager))))
                .Returns(ContentManager);

            PreviewMergeDataSyncer = A.Fake<IMergeDataSyncer>();
            PublishedMergeDataSyncer = A.Fake<IMergeDataSyncer>();

            PreviewAllowSync = A.Fake<IAllowSync>();
            A.CallTo(() => PreviewMergeDataSyncer.SyncAllowed(
                    A<IDataSyncReplicaSet>.That.Matches(s => s.Name == DataSyncReplicaSetNames.Preview),
                    A<ContentItem>._, A<IContentManager>._, A<IDataMergeContext?>._))
                .Returns(PreviewAllowSync);

            PublishedAllowSync = A.Fake<IAllowSync>();
            A.CallTo(() => PublishedMergeDataSyncer.SyncAllowed(
                    A<IDataSyncReplicaSet>.That.Matches(s => s.Name == DataSyncReplicaSetNames.Published),
                    A<ContentItem>._, A<IContentManager>._, A<IDataMergeContext?>._))
                .Returns(PublishedAllowSync);

            EventGridPublishingHandler = A.Fake<IContentOrchestrationHandler>();
            A.CallTo(() => EventGridPublishingHandler.Published(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.DraftSaved(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Cloned(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Unpublished(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.DraftDiscarded(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Deleted(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);

            SyncOrchestrator = new SyncOrchestrator(
                ContentDefinitionManager,
                Notifier,
                DataSyncCluster,
                ServiceProvider,
                Logger,
                PublishedContentItemVersion,
                new List<IContentOrchestrationHandler> { EventGridPublishingHandler });

            TestActivity = new Activity("UnitTest").Start();
        }

        public void Dispose()
        {
            TestActivity.Stop();
        }
    }
}
