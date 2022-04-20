using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Notifications;
using DFC.ServiceTaxonomy.DataSync.Orchestrators;
using DFC.ServiceTaxonomy.DataSync.Services;
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
        public IDataSyncNotifier Notifier { get; set; }
        public IPublishedContentItemVersion PublishedContentItemVersion { get; set; }
        public IPreviewContentItemVersion PreviewContentItemVersion { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public ILogger<DeleteOrchestrator> Logger { get; set; }

        public ContentItem ContentItem { get; set; }
        public IDeleteDataSyncer PreviewDeleteDataSyncer { get; set; }
        public IDeleteDataSyncer PublishedDeleteDataSyncer { get; set; }
        public IAllowSync PreviewAllowSync { get; set; }
        public IAllowSync PublishedAllowSync { get; set; }
        public IContentManager ContentManager { get; set; }
        public IContentOrchestrationHandler EventGridPublishingHandler { get; set; }
        public Activity TestActivity { get; set; }

        protected DeleteOrchestratorTestsBase()
        {
            ContentDefinitionManager = A.Fake<IContentDefinitionManager>();
            Notifier = A.Fake<DataSyncNotifier>();

            PublishedContentItemVersion = A.Fake<IPublishedContentItemVersion>();
            A.CallTo(() => PublishedContentItemVersion.DataSyncReplicaSetName)
                .Returns(DataSyncReplicaSetNames.Published);

            PreviewContentItemVersion = A.Fake<IPreviewContentItemVersion>();
            A.CallTo(() => PreviewContentItemVersion.DataSyncReplicaSetName)
                .Returns(DataSyncReplicaSetNames.Preview);

            ServiceProvider = A.Fake<IServiceProvider>();
            Logger = A.Fake<ILogger<DeleteOrchestrator>>();

            ContentItem = new ContentItem();

            ContentManager = A.Fake<IContentManager>();
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == nameof(IContentManager))))
                .Returns(ContentManager);

            PreviewDeleteDataSyncer = A.Fake<IDeleteDataSyncer>();
            PublishedDeleteDataSyncer = A.Fake<IDeleteDataSyncer>();

            PreviewAllowSync = A.Fake<IAllowSync>();
            A.CallTo(() => PreviewDeleteDataSyncer.DeleteAllowed(
                    A<ContentItem>._,
                    A<IContentItemVersion>.That.Matches(v => v.DataSyncReplicaSetName == DataSyncReplicaSetNames.Preview),
                    SyncOperation,
                    null,
                    null))
                .Returns(PreviewAllowSync);

            PublishedAllowSync = A.Fake<IAllowSync>();
            A.CallTo(() => PublishedDeleteDataSyncer.DeleteAllowed(
                    A<ContentItem>._,
                    A<IContentItemVersion>.That.Matches(v => v.DataSyncReplicaSetName == DataSyncReplicaSetNames.Published),
                    SyncOperation,
                    null,
                    null))
                .Returns(PublishedAllowSync);

            EventGridPublishingHandler = A.Fake<IContentOrchestrationHandler>();
            A.CallTo(() => EventGridPublishingHandler.Published(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.DraftSaved(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Cloned(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Unpublished(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.DraftDiscarded(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => EventGridPublishingHandler.Deleted(A<IOrchestrationContext>.Ignored)).Returns(Task.CompletedTask);

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
