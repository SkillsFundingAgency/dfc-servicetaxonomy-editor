using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Handlers.Orchestrators.SyncOrchestratorTests
{
    public class SyncOrchestrator_SaveDraftTests : SyncOrchestratorTestsBase
    {
        public SyncOrchestrator_SaveDraftTests()
        {
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == (nameof(IMergeGraphSyncer)))))
                .Returns(PreviewMergeGraphSyncer)
                .Once();
        }

        [Theory]
        [InlineData(SyncStatus.Allowed, false, true)]
        [InlineData(SyncStatus.Allowed, true, false)]
        [InlineData(SyncStatus.Blocked, null, false)]
        [InlineData(SyncStatus.NotRequired, null, true)]
        public async Task SaveDraft_SyncAllowedSyncMatrix_ReturnsBool(SyncStatus syncAllowedStatus, bool? syncToGraphReplicaSetThrows, bool expectedSuccess)
        {
            A.CallTo(() => PreviewAllowSyncResult.AllowSync)
                .Returns(syncAllowedStatus);

            if (syncToGraphReplicaSetThrows == true)
            {
                A.CallTo(() => PreviewMergeGraphSyncer.SyncToGraphReplicaSet())
                    .Throws(() => new Exception());
            }

            bool success = await SyncOrchestrator.SaveDraft(ContentItem);

            Assert.Equal(expectedSuccess, success);

            if (syncToGraphReplicaSetThrows == null)
            {
                A.CallTo(() => PreviewMergeGraphSyncer.SyncToGraphReplicaSet())
                    .MustNotHaveHappened();
            }
        }

        [Fact]
        public async Task SaveDraft_EventGridPublishingHandlerCalled()
        {
            bool success = await SyncOrchestrator.SaveDraft(ContentItem);

            Assert.True(success);

            A.CallTo(() => EventGridPublishingHandler.DraftSaved(ContentItem)).MustHaveHappened();
        }
    }
}
