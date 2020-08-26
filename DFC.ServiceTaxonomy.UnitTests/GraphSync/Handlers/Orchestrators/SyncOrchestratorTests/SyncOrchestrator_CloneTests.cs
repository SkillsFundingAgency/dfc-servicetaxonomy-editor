
using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Handlers.Orchestrators.SyncOrchestratorTests
{
    public class SyncOrchestrator_CloneTests : SyncOrchestratorTestsBase
    {
        public ICloneGraphSync CloneGraphSync { get; set; }

        public SyncOrchestrator_CloneTests()
        {
            CloneGraphSync = A.Fake<ICloneGraphSync>();
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == (nameof(ICloneGraphSync)))))
                .Returns(CloneGraphSync);
        }

        [Fact]
        public async Task Clone_CloneGraphSyncsMutateOnCloneCalled()
        {
            bool success = await SyncOrchestrator.Clone(ContentItem);

            Assert.True(success);

            A.CallTo(() => CloneGraphSync.MutateOnClone(ContentItem, ContentManager, null))
                .MustHaveHappened();
        }

        [Theory]
        [InlineData(SyncStatus.Allowed, false, true)]
        [InlineData(SyncStatus.Allowed, true, false)]
        [InlineData(SyncStatus.Blocked, null, false)]
        [InlineData(SyncStatus.NotRequired, null, true)]
        public async Task Clone_SyncAllowedSyncMatrix_ReturnsBool(SyncStatus syncAllowedStatus,
            bool? syncToGraphReplicaSetThrows, bool expectedSuccess)
        {
            A.CallTo(() => PreviewAllowSyncResult.AllowSync)
                .Returns(syncAllowedStatus);

            if (syncToGraphReplicaSetThrows == true)
            {
                A.CallTo(() => MergeGraphSyncer.SyncToGraphReplicaSet())
                    .Throws(() => new Exception());
            }

            bool success = await SyncOrchestrator.Clone(ContentItem);

            Assert.Equal(expectedSuccess, success);

            if (syncToGraphReplicaSetThrows == null)
            {
                A.CallTo(() => MergeGraphSyncer.SyncToGraphReplicaSet())
                    .MustNotHaveHappened();
            }
        }
    }
}
