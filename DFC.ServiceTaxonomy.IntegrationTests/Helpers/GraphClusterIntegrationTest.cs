using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    //todo: if use logger rather than helper, we can do checks like 'after disable no jobs started on disabled replica'
    // 'after disable, all in flight jobs on disabled replica finish' etc. by checking the cache of logs
    //todo: use interlocked for couter too, so round robin is more relicable
    public class GraphClusterIntegrationTest
    {
        internal const int NumberOfReplicasConfiguredForPublishedSet = 2;

        internal GraphClusterLowLevel GraphClusterLowLevel { get; }
        internal IEnumerable<INeoEndpoint> Endpoints { get; }
        internal ILogger<GraphClusterBuilder> GraphClusterLowLevelLogger { get; }
        internal ILogger<Graph> GraphLogger { get; }
        internal ILogger<GraphReplicaSetLowLevel> GraphReplicaSetLowLevelLogger { get; }

        internal GraphClusterIntegrationTest(
            GraphClusterCollectionFixture graphClusterCollectionFixture,
            ITestOutputHelper testOutputHelper)
        {
            Endpoints = graphClusterCollectionFixture.Neo4jOptions.Endpoints
                .Where(epc => epc.Enabled)
                .Select(epc => CreateFakeNeoEndpoint(epc.Name!));

            GraphReplicaSetLowLevelLogger = testOutputHelper.BuildLoggerFor<GraphReplicaSetLowLevel>();
            GraphLogger = testOutputHelper.BuildLoggerFor<Graph>();

            var graphReplicaSets = graphClusterCollectionFixture.Neo4jOptions.ReplicaSets
                .Select(rsc => new GraphReplicaSetLowLevel(
                    rsc.ReplicaSetName!,
                    ConstructGraphs(rsc, Endpoints, GraphLogger),
                    GraphReplicaSetLowLevelLogger));

            GraphClusterLowLevelLogger = testOutputHelper.BuildLoggerFor<GraphClusterBuilder>();

            GraphClusterLowLevel = new GraphClusterLowLevel(graphReplicaSets, GraphClusterLowLevelLogger);
        }

        //todo: can we trust TestOutputHelper.WriteLine to output in order?

        private INeoEndpoint CreateFakeNeoEndpoint(string name)
        {
            var neoEndpoint = A.Fake<INeoEndpoint>();

            A.CallTo(() => neoEndpoint.Name)
                .Returns(name);

            A.CallTo(() => neoEndpoint.Run(A<IQuery<int>[]>._, A<string>._, A<bool>._))
                .Invokes((IQuery<int>[] queries, string databaseName, bool defaultDatabase) =>
                {
                    GraphClusterLowLevelLogger.LogTrace($"Run started on {databaseName}, thread #{Thread.CurrentThread.ManagedThreadId}.");
                    Thread.Sleep(100);
                    GraphClusterLowLevelLogger.LogTrace($"Run finished on {databaseName}, thread #{Thread.CurrentThread.ManagedThreadId}.");
                })
                .Returns(new List<int> { 69 });

            return neoEndpoint;
        }

        private IEnumerable<Graph> ConstructGraphs(
            ReplicaSetConfiguration replicaSetConfiguration,
            IEnumerable<INeoEndpoint> neoEndpoints,
            ILogger<Graph> graphLogger)
        {
            return replicaSetConfiguration.GraphInstances
                .Where(gic => gic.Enabled)
                .Select((gic, index) =>
                    new Graph(
                        neoEndpoints.First(ep => ep.Name == gic.Endpoint),
                        gic.GraphName!,
                        gic.DefaultGraph,
                        index,
                        graphLogger));
        }

        protected void ReferenceCountTest(int parallelLoops)
        {
                const int replicaInstance = 0;

                var replicaSet = GraphClusterLowLevel.GetGraphReplicaSetLowLevel("published");

                Parallel.For(0, parallelLoops, (i, state) =>
                {
                    GraphClusterLowLevelLogger.LogTrace($"Thread id: {Thread.CurrentThread.ManagedThreadId}");

                    replicaSet.Disable(replicaInstance);
                    replicaSet.Enable(replicaInstance);
                });

                int enabledInstanceCount = replicaSet.EnabledInstanceCount();
                Assert.Equal(NumberOfReplicasConfiguredForPublishedSet, enabledInstanceCount);
                Assert.True(replicaSet.IsEnabled(replicaInstance));
        }
    }
}
