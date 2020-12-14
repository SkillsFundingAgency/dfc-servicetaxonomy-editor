using System;
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
    //todo: we need to unit test the neo services too
    [Collection("GraphCluster Integration")]
    public class GraphReplicaSetLowLevel_EnableTests : GraphClusterIntegrationTest
    {
        public GraphReplicaSetLowLevel_EnableTests(
            GraphClusterCollectionFixture graphClusterCollectionFixture,
            ITestOutputHelper testOutputHelper)
            : base(graphClusterCollectionFixture, testOutputHelper)
        {
        }

        [Fact]
        public void EnableReplicaTest()
        {
            const int totalIterations = 100;
            const int disableIteration = 5;
            const int reenableIteration = 30;

            const int reenableReplicaInstance = 0;
            const string disableReplicaInstanceGraphName = "neo4j";
            const string untouchedReplicaInstanceGraphName = "published1";

            var replicaSet = GraphClusterLowLevel.GetGraphReplicaSetLowLevel("published");

            Parallel.For(0, totalIterations, async (i, state) =>
            {
                switch (i)
                {
                    case disableIteration:
                        replicaSet.Disable(reenableReplicaInstance);
                        break;
                    case reenableIteration:
                        replicaSet.Enable(reenableReplicaInstance);
                        break;
                    default:
                        await replicaSet.Run(Query);
                        break;
                }
            });

            Logger.LogTrace(replicaSet.ToTraceString());

            List<LogEntry> log = Logger.Entries.ToList();

            (int? enabledLogEntryIndex, _) = GetLogEntry(log, LogId.GraphEnabled);

            // checking replica was re-enabled
            Assert.NotNull(enabledLogEntryIndex);

            ulong jobsStartedOnReEnabledReplica = (ulong)log
                .Skip(enabledLogEntryIndex!.Value + 1)
                .Count(l => IsLog(l, IntegrationTestLogId.RunQueryStarted,
                    kv => kv.Key == "DatabaseName" && (string)kv.Value == disableReplicaInstanceGraphName));

            ulong jobsStartedOnUntouchedReplica = (ulong)log
                .Skip(enabledLogEntryIndex!.Value + 1)
                .Count(l => IsLog(l, IntegrationTestLogId.RunQueryStarted,
                    kv => kv.Key == "DatabaseName" && (string)kv.Value == untouchedReplicaInstanceGraphName));

            Logger.LogInformation(
                $"Jobs started on replicas after reenable: reenabled={jobsStartedOnReEnabledReplica}, untouched={jobsStartedOnUntouchedReplica}.");

            Assert.True(jobsStartedOnReEnabledReplica > 0);
            Assert.True(jobsStartedOnUntouchedReplica > 0);
            const int allowableDifferenceBetweenJobStarts = 3;
            Assert.True(Math.Abs((int)jobsStartedOnReEnabledReplica-(int)jobsStartedOnUntouchedReplica) <= allowableDifferenceBetweenJobStarts,
                "Jobs started after replica reenable are roughly equal between the 2 replicas.");
        }
    }
}
