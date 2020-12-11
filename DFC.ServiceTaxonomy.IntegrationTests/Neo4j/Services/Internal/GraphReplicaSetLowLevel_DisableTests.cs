using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Log;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Divergic.Logging.Xunit;
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

        //todo: check any query run before disable() has returned is finished first
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
            List<LogEntry> log = Logger.Entries.ToList();

            int disabledLogEntryIndex = GetLogIndex(log, LogId.GraphDisabled);
            Assert.True(log
                    .Skip(disabledLogEntryIndex+1)
                    .All(l => !IsLog(l, IntegrationTestLogId.RunQueryStarted,
                        kv => kv.Key == "DatabaseName" && (string)kv.Value == replicaInstanceGraphName)),
                "No jobs were started on the disabled replica.");
        }

        private int GetLogIndex(List<LogEntry> log, LogId id)
        {
            return GetLogIndex(log, (int)id);
        }

        private int GetLogIndex(List<LogEntry> log, IntegrationTestLogId id)
        {
            return GetLogIndex(log, (int)id);
        }

        private int GetLogIndex(List<LogEntry> log, int logId)
        {
            return log.FindIndex(l => IsLog(l, logId));
        }

        private bool IsLog(LogEntry logEntry, LogId logId, Func<KeyValuePair<string, object>, bool>? stateCheck = null)
        {
            return IsLog(logEntry, (int)logId, stateCheck);
        }

        private bool IsLog(LogEntry logEntry, IntegrationTestLogId logId, Func<KeyValuePair<string, object>, bool>? stateCheck = null)
        {
            return IsLog(logEntry, (int)logId, stateCheck);
        }

        private bool IsLog(LogEntry logEntry, int logId, Func<KeyValuePair<string, object>, bool>? stateCheck = null)
        {
            return (logEntry.State as IReadOnlyList<KeyValuePair<string, object>>)?
                .Any(kv => kv.Key == "LogId" && (int)kv.Value == logId
                && (stateCheck?.Invoke(kv) ?? true)) == true;

        }
    }
}
