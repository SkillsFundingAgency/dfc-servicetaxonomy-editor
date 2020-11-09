using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Orchestrators.SyncOrchestratorTests
{
    public class SyncOrchestrator_UpdateTests : SyncOrchestratorTestsBase
    {
        public SyncOrchestrator_UpdateTests()
        {
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == (nameof(IMergeGraphSyncer)))))
                .Returns(PublishedMergeGraphSyncer).Once()
                .Then
                .Returns(PreviewMergeGraphSyncer).Once();
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
        public async Task Update_SyncAllowedSyncMatrix_ReturnsBool(
            AllowSyncResult publishedAllowSyncAllowedResult,
            AllowSyncResult previewAllowSyncAllowedResult,
            bool expectedSuccess)
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(publishedAllowSyncAllowedResult);

            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(previewAllowSyncAllowedResult);

            bool success = await SyncOrchestrator.Update(ContentItem, ContentItem);

            Assert.Equal(expectedSuccess, success);
        }

        //todo: SyncCalled theory needed
        //todo: ExceptionPropagates theory needed

        // we should only ever get NotRequired returned by both published and preview
        // not in conjunction with Allowed or Blocked
        //todo: add an exception guard for it?
        [Theory]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Allowed, 1, 1)]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.Blocked, 0, 0)]
        [InlineData(AllowSyncResult.Allowed, AllowSyncResult.NotRequired, 0, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Allowed, 0, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.Blocked, 0, 0)]
        [InlineData(AllowSyncResult.Blocked, AllowSyncResult.NotRequired, 0, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Allowed, 0, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.Blocked, 0, 0)]
        [InlineData(AllowSyncResult.NotRequired, AllowSyncResult.NotRequired, 0, 0)]
        public async Task Update_EventGridPublishingHandlerCalled(
            AllowSyncResult publishedAllowSyncResult,
            AllowSyncResult previewAllowSyncResult,
            int publishedCalled,
            int draftSavedCalled)
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(publishedAllowSyncResult);

            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(previewAllowSyncResult);

            await SyncOrchestrator.Update(ContentItem, ContentItem);

            A.CallTo(() => EventGridPublishingHandler.Published(ContentItem))
                .MustHaveHappened(publishedCalled, Times.Exactly);
            A.CallTo(() => EventGridPublishingHandler.DraftSaved(ContentItem))
                .MustHaveHappened(draftSavedCalled, Times.Exactly);
        }
    }
}
