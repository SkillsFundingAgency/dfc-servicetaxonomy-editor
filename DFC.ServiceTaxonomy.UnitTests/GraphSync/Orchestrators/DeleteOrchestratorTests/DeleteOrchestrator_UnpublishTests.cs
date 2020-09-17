using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Services;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Orchestrators.DeleteOrchestratorTests
{
    public class DeleteOrchestrator_UnpublishTests : DeleteOrchestratorTestsBase
    {
        protected override SyncOperation SyncOperation => SyncOperation.Unpublish;

        public DeleteOrchestrator_UnpublishTests()
        {
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == (nameof(IDeleteGraphSyncer)))))
                .Returns(PublishedDeleteGraphSyncer)
                .Once();
        }

        [Theory]
        [InlineData(AllowSyncResult.Allowed, true)]
        [InlineData(AllowSyncResult.Blocked, false)]
        [InlineData(AllowSyncResult.NotRequired, true)]
        public async Task DeleteDraft_SyncAllowedMatrix_ReturnsBool(
            AllowSyncResult allowSyncAllowedResult,
            bool expectedSuccess)
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(allowSyncAllowedResult);

            bool success = await DeleteOrchestrator.Unpublish(ContentItem);

            Assert.Equal(expectedSuccess, success);
        }

        [Theory]
        [InlineData(AllowSyncResult.Allowed, true)]
        [InlineData(AllowSyncResult.Blocked, false)]
        [InlineData(AllowSyncResult.NotRequired, false)]
        public async Task DeleteDraft_SyncAllowedMatrix_SyncCalled(
            AllowSyncResult allowSyncAllowedResult,
            bool expectedSyncCalled)
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(allowSyncAllowedResult);

            await DeleteOrchestrator.Unpublish(ContentItem);

            A.CallTo(() => PublishedDeleteGraphSyncer.Delete())
                .MustHaveHappened(expectedSyncCalled?1:0, Times.Exactly);
        }

        [Fact]
        public async Task DeleteDraft_MergeGraphSyncerThrows_ExceptionPropagates()
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(AllowSyncResult.Allowed);

            A.CallTo(() => PublishedDeleteGraphSyncer.Delete())
                .Throws(() => new Exception());

            await Assert.ThrowsAsync<Exception>(() => DeleteOrchestrator.Unpublish(ContentItem));
        }

        [Fact]
        public async Task DeleteDraft_EventGridPublishingHandlerCalled()
        {
            await DeleteOrchestrator.Unpublish(ContentItem);

            A.CallTo(() => EventGridPublishingHandler.Unpublished(ContentItem)).MustHaveHappened();
        }
    }
}
