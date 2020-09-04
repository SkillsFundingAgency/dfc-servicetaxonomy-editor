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
        [InlineData(SyncStatus.Allowed, SyncStatus.Allowed, false, false, true)]
        [InlineData(SyncStatus.Allowed, SyncStatus.Allowed, false, true, false)]
        [InlineData(SyncStatus.Allowed, SyncStatus.Allowed, true, false, false)]
        [InlineData(SyncStatus.Allowed, SyncStatus.Allowed, true, true, false)]
        [InlineData(SyncStatus.Allowed, SyncStatus.Blocked, null, null, false)]
        //todo: throw an exception, if either is notrequired, would expect them both to be notrequired?
        [InlineData(SyncStatus.Allowed, SyncStatus.NotRequired, false, null, true)]
        [InlineData(SyncStatus.Allowed, SyncStatus.NotRequired, true, null, false)]
        [InlineData(SyncStatus.Blocked, SyncStatus.Allowed, null, null, false)]
        [InlineData(SyncStatus.Blocked, SyncStatus.Blocked, null, null, false)]
        [InlineData(SyncStatus.Blocked, SyncStatus.NotRequired, null, null, false)]
        [InlineData(SyncStatus.NotRequired, SyncStatus.Allowed, null, false, true)]
        [InlineData(SyncStatus.NotRequired, SyncStatus.Allowed, null, true, false)]
        [InlineData(SyncStatus.NotRequired, SyncStatus.Blocked, null, null, false)]
        [InlineData(SyncStatus.NotRequired, SyncStatus.NotRequired, null, null, true)]
        public async Task Update_SyncAllowedSyncMatrix_ReturnsBool(
            SyncStatus publishedSyncAllowedStatus,
            SyncStatus previewSyncAllowedStatus,
            bool? publishedSyncToGraphReplicaSetThrows,
            bool? previewSyncToGraphReplicaSetThrows,
            bool expectedSuccess)
        {
            A.CallTo(() => PublishedAllowSyncResult.AllowSync)
                .Returns(publishedSyncAllowedStatus);

            A.CallTo(() => PreviewAllowSyncResult.AllowSync)
                .Returns(previewSyncAllowedStatus);

            if (publishedSyncToGraphReplicaSetThrows == true)
            {
                A.CallTo(() => PublishedMergeGraphSyncer.SyncToGraphReplicaSet())
                    .Throws(() => new Exception());
            }

            if (previewSyncToGraphReplicaSetThrows == true)
            {
                A.CallTo(() => PreviewMergeGraphSyncer.SyncToGraphReplicaSet())
                    .Throws(() => new Exception());
            }

            bool success = await SyncOrchestrator.Update(ContentItem, ContentItem);

            Assert.Equal(expectedSuccess, success);

            if (publishedSyncToGraphReplicaSetThrows == null)
            {
                A.CallTo(() => PublishedMergeGraphSyncer.SyncToGraphReplicaSet())
                    .MustNotHaveHappened();
            }

            if (previewSyncToGraphReplicaSetThrows == null)
            {
                A.CallTo(() => PreviewMergeGraphSyncer.SyncToGraphReplicaSet())
                    .MustNotHaveHappened();
            }
        }

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
