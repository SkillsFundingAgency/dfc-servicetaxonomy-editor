#if REPLICA_DISABLING_NET5_ONLY

using System;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Services.Internal
{
    public class GraphReplicaSetLowLevelTests : GraphReplicaSetTestsBase
    {
        internal GraphReplicaSetLowLevel GraphReplicaSet { get; set; }

        public GraphReplicaSetLowLevelTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            GraphReplicaSet = new GraphReplicaSetLowLevel(ReplicaSetName, GraphInstances, Logger);
        }

        [Fact]
        public void IsEnabled_InstanceDisabled_ReturnsFalse()
        {
            const int instanceToDisable = 4;
            GraphReplicaSet.Disable(instanceToDisable);

            Assert.False(GraphReplicaSet.IsEnabled(instanceToDisable));
        }

        [Fact]
        public void IsEnabled_InstanceReenabled_ReturnsTrue()
        {
            const int instanceToDisable = 4;
            GraphReplicaSet.Disable(instanceToDisable);
            GraphReplicaSet.Enable(instanceToDisable);

            Assert.True(GraphReplicaSet.IsEnabled(instanceToDisable));
        }

        // note: other related scenarios are covered in unit tests in GraphReplicaSetTests
        // and in the integration tests

        [Fact]
        public void Run_Query_LimitedToOneDisabledInstance_ExceptionThrown()
        {
            const int limitedToInstance = 2;
            GraphReplicaSet = new GraphReplicaSetLowLevel(ReplicaSetName, GraphInstances, Logger, limitedToInstance);

            GraphReplicaSet.Disable(limitedToInstance);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await GraphReplicaSet.Run(Query));
        }

        [Fact]
        public void Run_Command_LimitedToOneDisabledInstance_ExceptionThrown()
        {
            const int limitedToInstance = 2;
            GraphReplicaSet = new GraphReplicaSetLowLevel(ReplicaSetName, GraphInstances, Logger, limitedToInstance);

            GraphReplicaSet.Disable(limitedToInstance);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await GraphReplicaSet.Run(Command));
        }

        [Fact]
        public void Run_Command_ReplicaIsDisabled_ExceptionThrown()
        {
            const int instanceToDisable = 2;
            GraphReplicaSet.Disable(instanceToDisable);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await GraphReplicaSet.Run(Command));
        }

        [Fact]
        public void EnabledInstanceCount_ReplicaDisabled_CountAsExpected()
        {
            const int instanceToDisable = 2;
            GraphReplicaSet.Disable(instanceToDisable);
            int enabledInstances = GraphReplicaSet.EnabledInstanceCount();

            Assert.Equal(NumberOfGraphInstances-1, enabledInstances);
        }

        [Fact]
        public void EnabledInstanceCount_AllReplicaDisabled_CountIs0()
        {
            for (int instance = 0; instance < NumberOfGraphInstances; ++instance)
            {
                GraphReplicaSet.Disable(instance);
            }
            int enabledInstances = GraphReplicaSet.EnabledInstanceCount();

            Assert.Equal(0, enabledInstances);
        }
    }
}
#endif
