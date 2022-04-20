using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Models;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Services
{
    public class GraphReplicaSetTests : GraphReplicaSetTestsBase
    {
        internal DataSyncReplicaSet DataSyncReplicaSet { get; set; }

        public GraphReplicaSetTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            DataSyncReplicaSet = new DataSyncReplicaSet(ReplicaSetName, GraphInstances, Logger);
        }

        [Fact]
        public void Run_Query_EvenCallsAcrossReplicasTest()
        {
            const int iterationsPerGraphInstance = 5;
            const int numberOfIterations = NumberOfGraphInstances * iterationsPerGraphInstance;
            Parallel.For(0, numberOfIterations, async (iteration) =>
            {
                await DataSyncReplicaSet.Run(Query);
            });


            int index = 0;
            foreach (var graph in GraphInstances)
            {
                var calls = Fake.GetCalls(graph).ToList();

                TestOutputHelper.WriteLine($"DataSync #{index}: Run() called x{calls.Count}.");
                ++index;
            }

            foreach (var graph in GraphInstances)
            {
                A.CallTo(() => graph.Run(A<IQuery<int>[]>._))
                    .MustHaveHappened(iterationsPerGraphInstance, Times.Exactly);
            }
        }

        [Fact]
        public async Task Run_Query_LimitedToOneInstance_QueryIsRunOnCorrectInstance()
        {
            const int limitedToInstance = 2;
            DataSyncReplicaSet = new DataSyncReplicaSet(ReplicaSetName, GraphInstances, Logger, limitedToInstance);

            await DataSyncReplicaSet.Run(Query);

            int index = 0;
            foreach (var graph in GraphInstances)
            {
                var calls = Fake.GetCalls(graph).ToList();

                TestOutputHelper.WriteLine($"DataSync #{index}: Run() called x{calls.Count}.");
                ++index;
            }

            index = 0;
            foreach (var graph in GraphInstances)
            {
                A.CallTo(() => graph.Run(A<IQuery<int>[]>._))
                    .MustHaveHappened(index == limitedToInstance ? 1 : 0, Times.Exactly);
                ++index;
            }
        }

        [Fact]
        public async Task Run_Command_CallsEveryReplica()
        {
            await DataSyncReplicaSet.Run(Command);

            foreach (var graph in GraphInstances)
            {
                A.CallTo(() => graph.Run(A<ICommand[]>._))
                    .MustHaveHappened(1, Times.Exactly);
            }
        }

        [Fact]
        public async Task Run_Command_LimitedToOneInstance_CommandIsRunOnCorrectInstance()
        {
            const int limitedToInstance = 2;
            DataSyncReplicaSet = new DataSyncReplicaSet(ReplicaSetName, GraphInstances, Logger, limitedToInstance);

            await DataSyncReplicaSet.Run(Command);

            int index = 0;
            foreach (var graph in GraphInstances)
            {
                var calls = Fake.GetCalls(graph).ToList();

                TestOutputHelper.WriteLine($"DataSync #{index}: Run() called x{calls.Count}.");
                ++index;
            }

            index = 0;
            foreach (var graph in GraphInstances)
            {
                A.CallTo(() => graph.Run(A<ICommand>._))
                    .MustHaveHappened(index == limitedToInstance ? 1 : 0, Times.Exactly);
                ++index;
            }
        }

        [Fact]
        public void InstanceCount_MatchesNumberOfInstances()
        {
            Assert.Equal(NumberOfGraphInstances, DataSyncReplicaSet.InstanceCount);
        }

        [Fact]
        public void EnabledInstanceCount_MatchesNumberOfInstances()
        {
            Assert.Equal(NumberOfGraphInstances, DataSyncReplicaSet.EnabledInstanceCount());
        }

        [Fact]
        public void IsEnabled_AllGraphInstancesAreEnabled()
        {
            for (int instance = 0; instance < NumberOfGraphInstances; ++instance)
            {
                Assert.True(DataSyncReplicaSet.IsEnabled(instance));
            }
        }
    }
}
