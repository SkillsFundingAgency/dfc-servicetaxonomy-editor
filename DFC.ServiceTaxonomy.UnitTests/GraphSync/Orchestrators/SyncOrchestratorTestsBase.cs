using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
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
        public IGraphSyncNotifier Notifier { get; set; }
        public IGraphCluster GraphCluster { get; set; }
        public IPublishedContentItemVersion PublishedContentItemVersion { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public ILogger<SyncOrchestrator> Logger { get; set; }

        public ContentItem ContentItem { get; set; }
        public IMergeGraphSyncer PreviewMergeGraphSyncer { get; set; }
        public IMergeGraphSyncer PublishedMergeGraphSyncer { get; set; }
        public IAllowSync PreviewAllowSync { get; set; }
        public IAllowSync PublishedAllowSync { get; set; }
        public IContentManager ContentManager { get; set; }
        public IGraphReplicaSet PreviewGraphReplicaSet { get; set; }
        public IGraphReplicaSet PublishedGraphReplicaSet { get; set; }
        public IContentOrchestrationHandler EventGridPublishingHandler { get; set; }
        public Activity TestActivity { get; set; }

        public SyncOrchestratorTestsBase()
        {
            ContentDefinitionManager = A.Fake<IContentDefinitionManager>();
            Notifier = A.Fake<GraphSyncNotifier>();

            PreviewGraphReplicaSet = A.Fake<IGraphReplicaSet>();
            A.CallTo(() => PreviewGraphReplicaSet.Name)
                .Returns(GraphReplicaSetNames.Preview);

            PublishedGraphReplicaSet = A.Fake<IGraphReplicaSet>();
            A.CallTo(() => PublishedGraphReplicaSet.Name)
                .Returns(GraphReplicaSetNames.Published);

            var graphReplicaSets = new Dictionary<string, IGraphReplicaSet>
            {
                { GraphReplicaSetNames.Preview, PreviewGraphReplicaSet },
                { GraphReplicaSetNames.Published, PublishedGraphReplicaSet },
            };

            GraphCluster = A.Fake<IGraphCluster>();
            A.CallTo(() => GraphCluster.GetGraphReplicaSet(A<string>._))
                .ReturnsLazily<IGraphReplicaSet, string>(graphReplicaSetName => graphReplicaSets[graphReplicaSetName]);

            PublishedContentItemVersion = A.Fake<IPublishedContentItemVersion>();
            ServiceProvider = A.Fake<IServiceProvider>();
            Logger = A.Fake<ILogger<SyncOrchestrator>>();

            ContentItem = new ContentItem();

            ContentManager = A.Fake<IContentManager>();
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == nameof(IContentManager))))
                .Returns(ContentManager);

            PreviewMergeGraphSyncer = A.Fake<IMergeGraphSyncer>();
            PublishedMergeGraphSyncer = A.Fake<IMergeGraphSyncer>();

            PreviewAllowSync = A.Fake<IAllowSync>();
            A.CallTo(() => PreviewMergeGraphSyncer.SyncAllowed(
                    A<IGraphReplicaSet>.That.Matches(s => s.Name == GraphReplicaSetNames.Preview),
                    A<ContentItem>._, A<IContentManager>._, A<IGraphMergeContext?>._))
                .Returns(PreviewAllowSync);

            PublishedAllowSync = A.Fake<IAllowSync>();
            A.CallTo(() => PublishedMergeGraphSyncer.SyncAllowed(
                    A<IGraphReplicaSet>.That.Matches(s => s.Name == GraphReplicaSetNames.Published),
                    A<ContentItem>._, A<IContentManager>._, A<IGraphMergeContext?>._))
                .Returns(PublishedAllowSync);

            EventGridPublishingHandler = A.Fake<IContentOrchestrationHandler>();
            A.CallTo(() => EventGridPublishingHandler.Published(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.DraftSaved(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Cloned(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Unpublished(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.DraftDiscarded(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Deleted(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);

            SyncOrchestrator = new SyncOrchestrator(
                ContentDefinitionManager,
                Notifier,
                GraphCluster,
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
