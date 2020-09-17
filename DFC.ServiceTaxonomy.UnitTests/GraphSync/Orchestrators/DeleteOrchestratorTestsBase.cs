using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators;
using DFC.ServiceTaxonomy.GraphSync.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Orchestrators
{
    //todo: could have a common base class with Sync
    public abstract class DeleteOrchestratorTestsBase : IDisposable
    {
        protected abstract SyncOperation SyncOperation { get; }
        public DeleteOrchestrator DeleteOrchestrator { get; set; }
        public IContentDefinitionManager ContentDefinitionManager { get; set; }
        public IGraphSyncNotifier Notifier { get; set; }
        public IPublishedContentItemVersion PublishedContentItemVersion { get; set; }
        public IPreviewContentItemVersion PreviewContentItemVersion { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public ILogger<DeleteOrchestrator> Logger { get; set; }

        public ContentItem ContentItem { get; set; }
        public IDeleteGraphSyncer PreviewDeleteGraphSyncer { get; set; }
        public IDeleteGraphSyncer PublishedDeleteGraphSyncer { get; set; }
        public IAllowSync PreviewAllowSync { get; set; }
        public IAllowSync PublishedAllowSync { get; set; }
        public IContentManager ContentManager { get; set; }
        public IContentOrchestrationHandler EventGridPublishingHandler { get; set; }
        public Activity TestActivity { get; set; }

        protected DeleteOrchestratorTestsBase()
        {
            ContentDefinitionManager = A.Fake<IContentDefinitionManager>();
            Notifier = A.Fake<GraphSyncNotifier>();

            PublishedContentItemVersion = A.Fake<IPublishedContentItemVersion>();
            A.CallTo(() => PublishedContentItemVersion.GraphReplicaSetName)
                .Returns(GraphReplicaSetNames.Published);

            PreviewContentItemVersion = A.Fake<IPreviewContentItemVersion>();
            A.CallTo(() => PreviewContentItemVersion.GraphReplicaSetName)
                .Returns(GraphReplicaSetNames.Preview);

            ServiceProvider = A.Fake<IServiceProvider>();
            Logger = A.Fake<ILogger<DeleteOrchestrator>>();

            ContentItem = new ContentItem();

            ContentManager = A.Fake<IContentManager>();
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == nameof(IContentManager))))
                .Returns(ContentManager);

            PreviewDeleteGraphSyncer = A.Fake<IDeleteGraphSyncer>();
            PublishedDeleteGraphSyncer = A.Fake<IDeleteGraphSyncer>();

            PreviewAllowSync = A.Fake<IAllowSync>();
            A.CallTo(() => PreviewDeleteGraphSyncer.DeleteAllowed(
                    A<ContentItem>._,
                    A<IContentItemVersion>.That.Matches(v => v.GraphReplicaSetName == GraphReplicaSetNames.Preview),
                    SyncOperation,
                    null,
                    null))
                .Returns(PreviewAllowSync);

            PublishedAllowSync = A.Fake<IAllowSync>();
            A.CallTo(() => PublishedDeleteGraphSyncer.DeleteAllowed(
                    A<ContentItem>._,
                    A<IContentItemVersion>.That.Matches(v => v.GraphReplicaSetName == GraphReplicaSetNames.Published),
                    SyncOperation,
                    null,
                    null))
                .Returns(PublishedAllowSync);

            EventGridPublishingHandler = A.Fake<IContentOrchestrationHandler>();
            A.CallTo(() => EventGridPublishingHandler.Published(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.DraftSaved(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Cloned(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Unpublished(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.DraftDiscarded(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Deleted(A<ContentItem>.Ignored)).Returns(Task.CompletedTask);

            DeleteOrchestrator = new DeleteOrchestrator(
                ContentDefinitionManager,
                Notifier,
                ServiceProvider,
                Logger,
                PublishedContentItemVersion,
                PreviewContentItemVersion,
                new List<IContentOrchestrationHandler> { EventGridPublishingHandler });

            TestActivity = new Activity("UnitTest").Start();
        }

        public void Dispose()
        {
            TestActivity.Stop();
        }
    }
}
