using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Services;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Orchestrators.DeleteOrchestratorTests
{
    public class DeleteOrchestrator_DeleteTests : DeleteOrchestratorTestsBase
    {
        protected override SyncOperation SyncOperation => SyncOperation.Delete;

        public DeleteOrchestrator_DeleteTests()
        {
            //todo: required?
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == (nameof(IDeleteGraphSyncer)))))
                .Returns(PublishedDeleteGraphSyncer).Once()
                .Then
                .Returns(PreviewDeleteGraphSyncer).Once();
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
        public async Task Delete_SyncAllowedSyncMatrix_ReturnsBool(
            AllowSyncResult publishedAllowSyncAllowedResult,
            AllowSyncResult previewAllowSyncAllowedResult,
            bool expectedSuccess)
        {
            A.CallTo(() => PublishedAllowSync.Result)
                .Returns(publishedAllowSyncAllowedResult);

            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(previewAllowSyncAllowedResult);

            bool success = await DeleteOrchestrator.Delete(ContentItem);

            Assert.Equal(expectedSuccess, success);
        }

        //todo: SyncCalled theory needed
        //todo: ExceptionPropagates theory needed

        [Fact]
        public async Task Publish_EventGridPublishingHandlerCalled()
        {
            bool success = await DeleteOrchestrator.Delete(ContentItem);

            Assert.True(success);

            A.CallTo(() => EventGridPublishingHandler.Deleted(ContentItem)).MustHaveHappened();
        }
    }
}
