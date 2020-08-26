﻿using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Handlers.Orchestrators
{
    public class SyncOrchestratorTestsBase
    {
        public SyncOrchestrator SyncOrchestrator { get; set; }
        public IContentDefinitionManager ContentDefinitionManager { get; set; }
        public INotifier Notifier { get; set; }
        public IGraphCluster GraphCluster { get; set; }
        public IPublishedContentItemVersion PublishedContentItemVersion { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public ILogger<SyncOrchestrator> Logger { get; set; }

        public ContentItem ContentItem { get; set; }
        public IMergeGraphSyncer MergeGraphSyncer { get; set; }
        public IAllowSyncResult PreviewAllowSyncResult { get; set; }
        public IAllowSyncResult PublishedAllowSyncResult { get; set; }
        public IContentManager ContentManager { get; set; }
        public IGraphReplicaSet PreviewGraphReplicaSet { get; set; }
        public IGraphReplicaSet PublishedGraphReplicaSet { get; set; }

        public SyncOrchestratorTestsBase()
        {
            ContentDefinitionManager = A.Fake<IContentDefinitionManager>();
            Notifier = A.Fake<Notifier>();

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

            //todo: will this work for extension?
            MergeGraphSyncer = A.Fake<IMergeGraphSyncer>();
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == (nameof(IMergeGraphSyncer)))))
                .Returns(MergeGraphSyncer);

            PreviewAllowSyncResult = A.Fake<IAllowSyncResult>();
            A.CallTo(() => MergeGraphSyncer.SyncAllowed(
                    A<IGraphReplicaSet>.That.Matches(s => s.Name == GraphReplicaSetNames.Preview),
                    A<ContentItem>._, A<IContentManager>._, A<IGraphMergeContext?>._))
                .Returns(PreviewAllowSyncResult);

            PublishedAllowSyncResult = A.Fake<IAllowSyncResult>();
            A.CallTo(() => MergeGraphSyncer.SyncAllowed(
                    A<IGraphReplicaSet>.That.Matches(s => s.Name == GraphReplicaSetNames.Published),
                    A<ContentItem>._, A<IContentManager>._, A<IGraphMergeContext?>._))
                .Returns(PublishedAllowSyncResult);

            SyncOrchestrator = new SyncOrchestrator(
                ContentDefinitionManager,
                Notifier,
                GraphCluster,
                PublishedContentItemVersion,
                ServiceProvider,
                Logger);
        }
    }
}
