using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphClusterIntegrationTest
    {
        internal const int NumberOfReplicasConfiguredForPublishedSet = 2;

        internal GraphClusterLowLevel GraphClusterLowLevel { get; }
        internal IEnumerable<INeoEndpoint> Endpoints { get; }
        internal ITestOutputHelper TestOutputHelper { get; }
        internal ILogger<GraphClusterLowLevel> GraphClusterLowLevelLogger { get; }
        internal ILogger<Graph> GraphLogger { get; }

        internal GraphClusterIntegrationTest(
            GraphClusterCollectionFixture graphClusterCollectionFixture,
            ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;

            Endpoints = graphClusterCollectionFixture.Neo4jOptions.Endpoints
                .Where(epc => epc.Enabled)
                .Select(epc => CreateFakeNeoEndpoint(epc.Name!));

            var graphReplicaSets = graphClusterCollectionFixture.Neo4jOptions.ReplicaSets
                .Select(rsc =>
                    new GraphReplicaSetLowLevel(rsc.ReplicaSetName!, ConstructGraphs(rsc, Endpoints)));

            GraphClusterLowLevelLogger = testOutputHelper.BuildLoggerFor<GraphClusterLowLevel>();
            GraphLogger = testOutputHelper.BuildLoggerFor<Graph>();

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
                    TestOutputHelper.WriteLine($"Run started on {databaseName}, thread #{Thread.CurrentThread.ManagedThreadId}.");
                    Thread.Sleep(100);
                    TestOutputHelper.WriteLine($"Run finished on {databaseName}, thread #{Thread.CurrentThread.ManagedThreadId}.");
                })
                .Returns(new List<int> { 69 });

            return neoEndpoint;
        }

        private IEnumerable<Graph> ConstructGraphs(
            ReplicaSetConfiguration replicaSetConfiguration,
            IEnumerable<INeoEndpoint> neoEndpoints)
        {
            return replicaSetConfiguration.GraphInstances
                .Where(gic => gic.Enabled)
                .Select((gic, index) =>
                    new Graph(
                        neoEndpoints.First(ep => ep.Name == gic.Endpoint),
                        gic.GraphName!,
                        gic.DefaultGraph,
                        index,
                        GraphLogger));
        }

        internal void Trace(IGraphReplicaSetLowLevel replicaSet)
        {
            TestOutputHelper.WriteLine("replicaSet:");
            TestOutputHelper.WriteLine($"  InstanceCount={replicaSet.InstanceCount}, EnabledInstanceCount={replicaSet.EnabledInstanceCount}");
            for (int instance = 0; instance < replicaSet.InstanceCount; ++instance)
            {
                var graph = replicaSet.GraphInstances[instance];

                TestOutputHelper.WriteLine($"    Graph #{instance}: {graph.GraphName}");
                TestOutputHelper.WriteLine($"      IsEnabled()={replicaSet.IsEnabled(instance)}");
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
    }
}
