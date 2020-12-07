using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphClusterIntegrationTest //: IAsyncLifetime
    {
        public const int NumberOfReplicasConfiguredForPublishedSet = 2;

        public ITestOutputHelper TestOutputHelper { get; }
        private readonly GraphClusterCollectionFixture _graphClusterCollectionFixture;

        internal GraphClusterIntegrationTest(
            GraphClusterCollectionFixture graphClusterCollectionFixture,
            ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            _graphClusterCollectionFixture = graphClusterCollectionFixture;
        }

        internal GraphClusterLowLevel GraphClusterLowLevel
        {
            get
            {
                return _graphClusterCollectionFixture.GraphClusterLowLevel;
            }
        }

        protected void ReferenceCountTest(int parallelLoops)
        {
            const int replicaInstance = 0;

            var replicaSet = GraphClusterLowLevel.GetGraphReplicaSetLowLevel("published");

            Parallel.For(0, parallelLoops, (i, state) =>
            {
                TestOutputHelper.WriteLine($"Thread id: {Thread.CurrentThread.ManagedThreadId}");

                replicaSet.Disable(replicaInstance);
                replicaSet.Enable(replicaInstance);
            });

            int enabledInstanceCount = replicaSet.EnabledInstanceCount;
            Assert.Equal(NumberOfReplicasConfiguredForPublishedSet, enabledInstanceCount);
            Assert.True(replicaSet.IsEnabled(replicaInstance));
        }

        // public Task InitializeAsync()
        // {
        //     return Task.CompletedTask;
        // }
        //
        // public Task DisposeAsync()
        // {
        //     return Task.CompletedTask;
        // }
    }
}
