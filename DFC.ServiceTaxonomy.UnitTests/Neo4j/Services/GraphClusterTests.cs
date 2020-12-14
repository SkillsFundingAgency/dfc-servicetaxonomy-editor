using System.Collections.Generic;
using System.Linq;
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
        internal List<IGraphReplicaSetLowLevel> ReplicaSets { get; set; }
        internal ILogger Logger { get; set; }
        internal IQuery<int>[] Queries { get; set; }
        internal ICommand[] Commands { get; set; }
        internal const int NumberOfReplicaSets = 10;

        public GraphClusterTests()
        {
            ReplicaSets = new List<IGraphReplicaSetLowLevel>();
            for (int replicaSetOrdinal = 0; replicaSetOrdinal < NumberOfReplicaSets; ++replicaSetOrdinal)
            {
                var replicaSet = A.Fake<IGraphReplicaSetLowLevel>();
                A.CallTo(() => replicaSet.Name).Returns(replicaSetOrdinal.ToString());
                ReplicaSets.Add(replicaSet);
            }

            Logger = A.Fake<ILogger>();

            GraphCluster = new GraphCluster(ReplicaSets, Logger);

            Queries = new[] {A.Fake<IQuery<int>>()};
            Commands = new[] {A.Fake<ICommand>()};
        }

        [Fact]
        public void Run_Queries_RunOnNamedReplicaSet()
        {
            const int replicaToRunOn = 5;
            GraphCluster.Run(replicaToRunOn.ToString(), Queries);

            var expectedReplica = ReplicaSets.Skip(replicaToRunOn).First();
            A.CallTo(() => expectedReplica.Run(Queries))
                .MustHaveHappenedOnceExactly();
        }
    }
}
