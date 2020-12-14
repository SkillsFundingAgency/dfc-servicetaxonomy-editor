using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Services
{
    public class GraphReplicaSetTests
    {
        internal readonly ITestOutputHelper TestOutputHelper;
        internal GraphReplicaSet GraphReplicaSet { get; set; }
        internal List<Graph> GraphInstances { get; set; }
        internal ILogger Logger { get; set; }
        internal IQuery<int> Query { get; set; }

        internal const string ReplicaSetName = "ReplicaSetName";
        const int NumberOfGraphInstances = 10;

        public GraphReplicaSetTests(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            GraphInstances = new List<Graph>();
            for (int graphInstanceOrdinal = 0; graphInstanceOrdinal < NumberOfGraphInstances; ++graphInstanceOrdinal)
            {
                GraphInstances.Add(A.Fake<Graph>());
            }
            Logger = A.Fake<ILogger>();
            GraphReplicaSet = new GraphReplicaSet(ReplicaSetName, GraphInstances, Logger);

            Query = A.Fake<IQuery<int>>();
        }

        [Fact]
        public void Run_EvenCallsAcrossReplicasTest()
        {
            const int iterationsPerGraphInstance = 5;
            const int numberOfIterations = NumberOfGraphInstances * iterationsPerGraphInstance;
            Parallel.For(0, numberOfIterations, async (iteration) =>
            {
                await GraphReplicaSet.Run(Query);
            });


            int index = 0;
            foreach (var graph in GraphInstances)
            {
                var calls = Fake.GetCalls(graph).ToList();

                TestOutputHelper.WriteLine($"Graph #{index}: Run() called x{calls.Count}.");
                ++index;
            }

            foreach (var graph in GraphInstances)
            {
                A.CallTo(() => graph.Run(A<IQuery<int>[]>._))
                    .MustHaveHappened(iterationsPerGraphInstance, Times.Exactly);
            }
        }
    }
}
