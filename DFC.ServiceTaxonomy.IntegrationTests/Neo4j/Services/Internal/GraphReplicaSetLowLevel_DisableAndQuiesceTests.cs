#if REPLICA_DISABLING_NET5_ONLY

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Log;
using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j.Services.Internal
{
    [Collection("GraphCluster Integration")]
    public class GraphReplicaSetLowLevel_DisableAndQuiesceTests : GraphClusterIntegrationTest
    {
        public GraphReplicaSetLowLevel_DisableAndQuiesceTests(
            GraphClusterCollectionFixture graphClusterCollectionFixture,
            ITestOutputHelper testOutputHelper)
            : base(graphClusterCollectionFixture, testOutputHelper)
        {
        }

        // we could split this test out into multiple tests,
        // but it takes a while to run, so we combine multiple tests into one
        [Fact]
        public void DisableAndQuiesceReplicaTest()
        {
            const int disableReplicaInstance = 0;
            const string disableReplicaInstanceGraphName = "neo4j";

            var replicaSet = GraphClusterLowLevel.GetGraphReplicaSetLowLevel("published");

            Parallel.For(0, 100, async (i, state) =>
            {
                if (i == 5)
                {
                    replicaSet.Disable(disableReplicaInstance);
                }
                else
                {
                    await replicaSet.Run(Query);
                }
            });

            Logger.LogTrace(replicaSet.ToTraceString());

            List<LogEntry> log = Logger.Entries.ToList();

            (int? disabledLogEntryIndex, _) = GetLogEntry(log, LogId.GraphDisabled);

            // checking replica was disabled
            Assert.NotNull(disabledLogEntryIndex);

            Assert.True(log
                    .Skip(disabledLogEntryIndex!.Value+1)
                    .All(l => !IsLog(l, IntegrationTestLogId.RunQueryStarted,
                        kv => kv.Key == "DatabaseName" && (string)kv.Value == disableReplicaInstanceGraphName)),
                "No jobs were started on the disabled replica.");

            (int? quiescingLogEntryIndex, LogEntry? quiescingLogEntry) = GetLogEntry(log, LogId.QuiescingGraph);

            // checking replica started quiescing
            Assert.NotNull(quiescingLogEntryIndex);

            ulong inFlightCount = Get<ulong>(quiescingLogEntry!, "InFlightCount");

            ulong jobsFinishedOnDisabledReplicaAfterReplicaDisabled = (ulong)log
                .Skip(disabledLogEntryIndex!.Value + 1)
                .Count(l => IsLog(l, IntegrationTestLogId.RunQueryFinished,
                    kv => kv.Key == "DatabaseName" && (string)kv.Value == disableReplicaInstanceGraphName));

            // check right number of In flight jobs finished after replica was disabled
            Assert.Equal(inFlightCount, jobsFinishedOnDisabledReplicaAfterReplicaDisabled);
        }
    }
}
#endif
