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
        //public void DisableWaitsForInFlightQueriesTest()
        public void NoJobsAreStartedOnDisabledGraphReplica()
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

            //todo: use tags so not coupled to log message?
            List<LogEntry> log = Logger.Entries.ToList();

            (int? disabledLogEntryIndex, _) = GetLogEntry(log, LogId.GraphDisabled);

            // checking replica was disabled
            Assert.NotNull(disabledLogEntryIndex);

            Assert.True(log
                    .Skip(disabledLogEntryIndex!.Value+1)
                    .All(l => !IsLog(l, IntegrationTestLogId.RunQueryStarted,
                        kv => kv.Key == "DatabaseName" && (string)kv.Value == disableReplicaInstanceGraphName)),
                "No jobs were started on the disabled replica.");

            //todo: return entry and index
            (int? quiescingLogEntryIndex, LogEntry? quiescingLogEntry) = GetLogEntry(log, LogId.QuiescingGraph);

            // checking replica started quiescing
            Assert.NotNull(quiescingLogEntryIndex);

            ulong inFlightCount = Get<ulong>(quiescingLogEntry!, "InFlightCount");

            ulong jobsFinishedOnDisabledReplicaAfterReplicaDisabled = (ulong)log
                .Skip(disabledLogEntryIndex!.Value + 1)
                .Count(l => IsLog(l, IntegrationTestLogId.RunQueryFinished,
                    kv => kv.Key == "DatabaseName" && (string)kv.Value == disableReplicaInstanceGraphName));

            // check right number of In flight jobs finished after replica was disabled.
            Assert.Equal(inFlightCount, jobsFinishedOnDisabledReplicaAfterReplicaDisabled);
        }

        private (int?, LogEntry?) GetLogEntry(List<LogEntry> log, LogId id)
        {
            return GetLogEntry(log, (int)id);
        }

        private (int?, LogEntry?) GetLogEntry(List<LogEntry> log, IntegrationTestLogId id)
        {
            return GetLogEntry(log, (int)id);
        }

        private (int?, LogEntry?) GetLogEntry(List<LogEntry> log, int logId)
        {
            int index = log.FindIndex(l => IsLog(l, logId));
            if (index == -1)
                return (null, null);

            return (index, log[index]);
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
            if (!(logEntry.State is IReadOnlyList<KeyValuePair<string, object>> state))
                return false;

            if (!state.Any(kv => kv.Key == "LogId" && (int)kv.Value == logId))
                return false;

            return stateCheck == null || state.Any(stateCheck);
        }

        private T? Get<T>(LogEntry logEntry, string key)
        {
            #pragma warning disable S1905
            return (T?)((KeyValuePair<string, object>?)(logEntry.State as IReadOnlyList<KeyValuePair<string, object>>)?
                .FirstOrDefault(kv => kv.Key == key))?
                .Value;
            #pragma warning restore S1905
        }
    }
}
