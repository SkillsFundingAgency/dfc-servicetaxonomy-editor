using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Handlers.Orchestrators.SyncOrchestratorTests
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
        [InlineData(SyncStatus.Allowed, SyncStatus.Allowed, true)]
        [InlineData(SyncStatus.Allowed, SyncStatus.Blocked, false)]
        //todo: throw an exception, if either is notrequired, would expect them both to be notrequired?
        [InlineData(SyncStatus.Allowed, SyncStatus.NotRequired, true)]
        [InlineData(SyncStatus.Blocked, SyncStatus.Allowed, false)]
        [InlineData(SyncStatus.Blocked, SyncStatus.Blocked, false)]
        [InlineData(SyncStatus.Blocked, SyncStatus.NotRequired, false)]
        [InlineData(SyncStatus.NotRequired, SyncStatus.Allowed, true)]
        [InlineData(SyncStatus.NotRequired, SyncStatus.Blocked, false)]
        [InlineData(SyncStatus.NotRequired, SyncStatus.NotRequired, true)]
        public async Task Update_SyncAllowedSyncMatrix_ReturnsBool(
            SyncStatus publishedSyncAllowedStatus,
            SyncStatus previewSyncAllowedStatus,
            bool expectedSuccess)
        {
            A.CallTo(() => PublishedAllowSyncResult.AllowSync)
                .Returns(publishedSyncAllowedStatus);

            A.CallTo(() => PreviewAllowSyncResult.AllowSync)
                .Returns(previewSyncAllowedStatus);

            bool success = await SyncOrchestrator.Update(ContentItem, ContentItem);

            Assert.Equal(expectedSuccess, success);
        }

        //todo: SyncCalled theory needed
        //todo: ExceptionPropagates theory needed

        [Fact]
        public async Task Update_EventGridPublishingHandlerCalled()
        {
            bool success = await SyncOrchestrator.Update(ContentItem, ContentItem);

            Assert.True(success);

            A.CallTo(() => EventGridPublishingHandler.DraftSaved(ContentItem)).MustHaveHappened();
            A.CallTo(() => EventGridPublishingHandler.Published(ContentItem)).MustHaveHappened();
        }
    }
}
