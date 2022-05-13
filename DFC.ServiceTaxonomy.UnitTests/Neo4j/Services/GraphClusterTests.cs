using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Services
{
    public class GraphClusterTests
    {
        internal GraphCluster GraphCluster { get; set; }
        internal IGraphReplicaSetLowLevel[] ReplicaSets { get; set; }
        internal ILogger Logger { get; set; }
        internal IQuery<int>[] Queries { get; set; }
        internal ICommand[] Commands { get; set; }
        internal const int NumberOfReplicaSets = 10;

        public GraphClusterTests()
        {
            ReplicaSets = new IGraphReplicaSetLowLevel[NumberOfReplicaSets];
            for (int replicaSetOrdinal = 0; replicaSetOrdinal < NumberOfReplicaSets; ++replicaSetOrdinal)
            {
                var replicaSet = ReplicaSets[replicaSetOrdinal] = A.Fake<IGraphReplicaSetLowLevel>();
                A.CallTo(() => replicaSet.Name).Returns(replicaSetOrdinal.ToString());
            }

            Logger = A.Fake<ILogger>();

            GraphCluster = new GraphCluster(ReplicaSets, Logger);

            Queries = new[] {A.Fake<IQuery<int>>()};
            Commands = new[] {A.Fake<ICommand>()};
        }

        [Fact]
        public void Run_Queries_OnlyRunsOnNamedReplicaSet()
        {
            const int replicaToRunOn = 5;
            GraphCluster.Run(replicaToRunOn.ToString(), Queries);

            for (int replicaSetOrdinal = 0; replicaSetOrdinal < NumberOfReplicaSets; ++replicaSetOrdinal)
            {
                var expectedReplica = ReplicaSets[replicaSetOrdinal];

                A.CallTo(() => expectedReplica.Run(Queries))
                    .MustHaveHappened(replicaSetOrdinal == replicaToRunOn ? 1 : 0, Times.Exactly);
            }
        }

        [Fact]
        public void Run_Commands_OnlyRunsOnNamedReplicaSet()
        {
            const int replicaToRunOn = 5;
            GraphCluster.Run(replicaToRunOn.ToString(), Commands);

            for (int replicaSetOrdinal = 0; replicaSetOrdinal < NumberOfReplicaSets; ++replicaSetOrdinal)
            {
                var expectedReplica = ReplicaSets[replicaSetOrdinal];

                A.CallTo(() => expectedReplica.Run(Commands))
                    .MustHaveHappened(replicaSetOrdinal == replicaToRunOn ? 1 : 0, Times.Exactly);
            }
        }
    }
}
