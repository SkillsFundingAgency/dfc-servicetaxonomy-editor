using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Handlers.Orchestrators.SyncOrchestratorTests
{
    public class SyncOrchestrator_SaveDraftTests : SyncOrchestratorTestsBase
    {
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
                A.CallTo(() => MergeGraphSyncer.SyncToGraphReplicaSet())
                    .Throws(() => new Exception());
            }

            bool success = await SyncOrchestrator.SaveDraft(ContentItem);

            Assert.Equal(expectedSuccess, success);

            if (syncToGraphReplicaSetThrows == null)
            {
                A.CallTo(() => MergeGraphSyncer.SyncToGraphReplicaSet())
                    .MustNotHaveHappened();
            }
        }
    }
}
