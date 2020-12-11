using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j.Services.Internal
{
    [Collection("GraphCluster Integration")]
    public class GraphReplicaSetLowLevel_DisableTests : GraphClusterIntegrationTest
    {
        public IQuery<int> Query { get; }

        public GraphReplicaSetLowLevel_DisableTests(
            GraphClusterCollectionFixture graphClusterCollectionFixture,
            ITestOutputHelper testOutputHelper)
            : base(graphClusterCollectionFixture, testOutputHelper)
        {
            Query = A.Fake<IQuery<int>>();
        }

        // manual test (todo: partly automate)
        // checks: (might be a few stragglers, due to delay before trace lines are written)
        //todo: check only enabled replica used after disable() has returned - could be automated?
        //check any query run before disable() has returned is finished first
        //todo: out output helper loggers cache all lines, so we can use that in the assert phase
        [Fact]
        public void DisableWaitsForInFlightQueriesTest()
        {
            const int replicaInstance = 0;
            const string replicaInstanceGraphName = "neo4j";

            var replicaSet = GraphClusterLowLevel.GetGraphReplicaSetLowLevel("published");

            Parallel.For(0, 100, async (i, state) =>
            {
                if (i == 5)
                {
                    replicaSet.Disable(replicaInstance);
                }
                else
                {
                    await replicaSet.Run(Query);
                }
            });

            Logger.LogTrace(replicaSet.ToTraceString());

            //todo: use tags so not coupled to log message?
            var log = Logger.Entries.ToList();

            //todo: use state with enum
            int disabledLogEntryIndex = log.FindIndex(l => l.Message.Contains("Disabled graph"));
            Assert.True(log.Skip(disabledLogEntryIndex).All(l => l.Message != $"Run started on {replicaInstanceGraphName}"),
                "No jobs were started on the disabled replica.");

            // what can we check?, we can't assume all Run()'s before the disabled were started
            // and we can't assume Run()'s from later iteration's weren't started
            // int enabledInstanceCount = replicaSet.EnabledInstanceCount;
            // Assert.Equal(NumberOfReplicasConfiguredForPublishedSet, enabledInstanceCount);
        }
    }
}
