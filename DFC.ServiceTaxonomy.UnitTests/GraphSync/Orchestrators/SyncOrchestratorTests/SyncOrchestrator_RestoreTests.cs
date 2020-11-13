using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Orchestrators.SyncOrchestratorTests
{
    public class SyncOrchestrator_RestoreTests : SyncOrchestratorTestsBase
    {
        public SyncOrchestrator_RestoreTests()
        {
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == (nameof(IMergeGraphSyncer)))))
                .Returns(PreviewMergeGraphSyncer);
        }

        //[Theory]
        // [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Allowed, true)]
        // [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Blocked, false)]
        // //todo: throw an exception, if either is notrequired, would expect them both to be notrequired?
        // [InlineData(AllowSyncResult.Allowed, AllowSyncResult.NotRequired, true)]
        // [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Allowed, false)]
        // [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Blocked, false)]
        // [InlineData(AllowSyncResult.Blocked, AllowSyncResult.NotRequired, false)]
        // [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Allowed, true)]
        // [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Blocked, false)]
        // [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.NotRequired, true)]
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

        //todo: SyncCalled theory needed
        //todo: ExceptionPropagates theory needed

        // we should only ever get NotRequired returned by both published and preview
        // not in conjunction with Allowed or Blocked
        //todo: add an exception guard for it?
        //[Theory]
        // [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Allowed, 1)]
        // [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Blocked, 0)]
        // [InlineData(AllowSyncResult.Allowed, AllowSyncResult.NotRequired, 0)]
        // [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Allowed, 0)]
        // [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Blocked, 0)]
        // [InlineData(AllowSyncResult.Blocked, AllowSyncResult.NotRequired, 0)]
        // [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Allowed, 0)]
        // [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Blocked, 0)]
        // [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.NotRequired, 0)]
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

            A.CallTo(() => EventGridPublishingHandler.Published(
                    A<IOrchestrationContext>.That.Matches(ctx => Equals(ctx.ContentItem, ContentItem))))
                .MustHaveHappened(publishedCalled, Times.Exactly);
        }
    }
}
