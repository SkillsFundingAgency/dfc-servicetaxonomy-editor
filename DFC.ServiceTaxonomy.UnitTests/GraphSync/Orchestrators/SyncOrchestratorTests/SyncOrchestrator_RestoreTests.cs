using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Services;
using FakeItEasy;
using OrchardCore.ContentManagement;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Orchestrators.SyncOrchestratorTests
{
    public class SyncOrchestrator_RestoreTests : SyncOrchestratorTestsBase
    {
        public SyncOperation SyncOperation { get; }
        public IDeleteDataSyncer PublishedDeleteDataSyncer { get; set; }

        public SyncOrchestrator_RestoreTests()
        {
            SyncOperation = SyncOperation.Restore;

            PublishedDeleteDataSyncer = A.Fake<IDeleteDataSyncer>();

            A.CallTo(() => PublishedDeleteDataSyncer.DeleteAllowed(
                    A<ContentItem>._,
                    A<IContentItemVersion>.That.Matches(v => v.DataSyncReplicaSetName == DataSyncReplicaSetNames.Published),
                    SyncOperation,
                    null,
                    null))
                .Returns(PublishedAllowSync);

            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == (nameof(IMergeDataSyncer)))))
                .Returns(PreviewMergeDataSyncer);

            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == (nameof(IDeleteDataSyncer)))))
                .Returns(PublishedDeleteDataSyncer);
        }

        [Theory]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Allowed, true)]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Blocked, false)]
        //todo: throw an exception, if either is notrequired, would expect them both to be notrequired?
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.NotRequired, true)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Allowed, false)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Blocked, false)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.NotRequired, false)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Allowed, true)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Blocked, false)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.NotRequired, true)]
        public async Task Restore_SyncAllowedSyncMatrix_ReturnsBool(
            AllowSyncResult publishedAllowSyncAllowedResult,
            AllowSyncResult previewAllowSyncAllowedResult,
            bool expectedSuccess)
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(publishedAllowSyncAllowedResult);

            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(previewAllowSyncAllowedResult);

            bool success = await SyncOrchestrator.Restore(ContentItem);

            Assert.Equal(expectedSuccess, success);
        }

        [Theory]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Allowed, 1)]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Blocked, 0)]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.NotRequired, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Allowed, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Blocked, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.NotRequired, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Allowed, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Blocked, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.NotRequired, 0)]
        public async Task Restore_SyncToGraphReplicaSetCalled(
            AllowSyncResult publishedAllowSyncResult,
            AllowSyncResult previewAllowSyncResult,
            int publishedCalled)
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(publishedAllowSyncResult);

            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(previewAllowSyncResult);

            await SyncOrchestrator.Restore(ContentItem);

            A.CallTo(() => PreviewMergeDataSyncer.SyncToDataSyncReplicaSet())
                .MustHaveHappened(publishedCalled, Times.Exactly);
        }

        [Theory]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Allowed, 1)]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Blocked, 0)]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.NotRequired, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Allowed, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Blocked, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.NotRequired, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Allowed, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Blocked, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.NotRequired, 0)]
        public async Task Restore_DeleteCalled(
            AllowSyncResult publishedAllowSyncResult,
            AllowSyncResult previewAllowSyncResult,
            int publishedCalled)
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(publishedAllowSyncResult);

            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(previewAllowSyncResult);

            await SyncOrchestrator.Restore(ContentItem);

            A.CallTo(() => PublishedDeleteDataSyncer.Delete())
                .MustHaveHappened(publishedCalled, Times.Exactly);
        }

        [Fact]
        public Task Restore_PreviewSyncToGraphReplicaSetThrows_ExceptionPropagates()
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(AllowSyncResult.Allowed);

            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(AllowSyncResult.Allowed);

            A.CallTo(() => PreviewMergeDataSyncer.SyncToDataSyncReplicaSet())
                .Throws(() => new Exception());

            return Assert.ThrowsAsync<Exception>(() => SyncOrchestrator.Restore(ContentItem));
        }

        [Fact]
        public Task Restore_PublishedDeleteThrows_ExceptionPropagates()
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(AllowSyncResult.Allowed);

            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(AllowSyncResult.Allowed);

            A.CallTo(() => PublishedDeleteDataSyncer.Delete())
                .Throws(() => new Exception());

            return Assert.ThrowsAsync<Exception>(() => SyncOrchestrator.Restore(ContentItem));
        }

        // we should only ever get NotRequired returned by both published and preview
        // not in conjunction with Allowed or Blocked
        //todo: add an exception guard for it?
        [Theory]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Allowed, 1)]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Blocked, 0)]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.NotRequired, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Allowed, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Blocked, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.NotRequired, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Allowed, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Blocked, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.NotRequired, 0)]
        public async Task Restore_EventGridPublishingHandlerCalled(
            AllowSyncResult publishedAllowSyncResult,
            AllowSyncResult previewAllowSyncResult,
            int publishedCalled)
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(publishedAllowSyncResult);

            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(previewAllowSyncResult);

            await SyncOrchestrator.Restore(ContentItem);

            A.CallTo(() => EventGridPublishingHandler.Restored(
                    A<IOrchestrationContext>.That.Matches(ctx => Equals(ctx.ContentItem, ContentItem))))
                .MustHaveHappened(publishedCalled, Times.Exactly);
        }
    }
}
