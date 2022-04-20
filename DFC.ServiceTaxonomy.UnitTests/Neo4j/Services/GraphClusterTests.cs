using DFC.ServiceTaxonomy.DataSync.CosmosDb;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Services
{
    public class GraphClusterTests
    {
        internal CosmosDbDataSyncCluster DataSyncCluster { get; set; }
        internal IDataSyncReplicaSetLowLevel[] ReplicaSets { get; set; }
        internal ILogger Logger { get; set; }
        internal IQuery<int>[] Queries { get; set; }
        internal ICommand[] Commands { get; set; }
        internal const int NumberOfReplicaSets = 10;

        public GraphClusterTests()
        {
            ReplicaSets = new IDataSyncReplicaSetLowLevel[NumberOfReplicaSets];
            for (int replicaSetOrdinal = 0; replicaSetOrdinal < NumberOfReplicaSets; ++replicaSetOrdinal)
            {
                var replicaSet = ReplicaSets[replicaSetOrdinal] = A.Fake<IDataSyncReplicaSetLowLevel>();
                A.CallTo(() => replicaSet.Name).Returns(replicaSetOrdinal.ToString());
            }

            Logger = A.Fake<ILogger>();

            DataSyncCluster = new CosmosDbDataSyncCluster(ReplicaSets);

            Queries = new[] {A.Fake<IQuery<int>>()};
            Commands = new[] {A.Fake<ICommand>()};
        }

        [Fact]
        public void Run_Queries_OnlyRunsOnNamedReplicaSet()
        {
            const int replicaToRunOn = 5;
            DataSyncCluster.Run(replicaToRunOn.ToString(), Queries);

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
            DataSyncCluster.Run(replicaToRunOn.ToString(), Commands);

            for (int replicaSetOrdinal = 0; replicaSetOrdinal < NumberOfReplicaSets; ++replicaSetOrdinal)
            {
                var expectedReplica = ReplicaSets[replicaSetOrdinal];

                A.CallTo(() => expectedReplica.Run(Commands))
                    .MustHaveHappened(replicaSetOrdinal == replicaToRunOn ? 1 : 0, Times.Exactly);
            }
        }
    }
}
