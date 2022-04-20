#if REPLICA_DISABLING_NET5_ONLY

using System;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Services.Internal
{
    public class GraphReplicaSetLowLevelTests : GraphReplicaSetTestsBase
    {
        internal DataSyncReplicaSetLowLevel DataSyncReplicaSet { get; set; }

        public GraphReplicaSetLowLevelTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            DataSyncReplicaSet = new DataSyncReplicaSetLowLevel(ReplicaSetName, DataSyncInstances, Logger);
        }

        [Fact]
        public void IsEnabled_InstanceDisabled_ReturnsFalse()
        {
            const int instanceToDisable = 4;
            DataSyncReplicaSet.Disable(instanceToDisable);

            Assert.False(DataSyncReplicaSet.IsEnabled(instanceToDisable));
        }

        [Fact]
        public void IsEnabled_InstanceReenabled_ReturnsTrue()
        {
            const int instanceToDisable = 4;
            DataSyncReplicaSet.Disable(instanceToDisable);
            DataSyncReplicaSet.Enable(instanceToDisable);

            Assert.True(DataSyncReplicaSet.IsEnabled(instanceToDisable));
        }

        // note: other related scenarios are covered in unit tests in GraphReplicaSetTests
        // and in the integration tests

        [Fact]
        public void Run_Query_LimitedToOneDisabledInstance_ExceptionThrown()
        {
            const int limitedToInstance = 2;
            DataSyncReplicaSet = new DataSyncReplicaSetLowLevel(ReplicaSetName, DataSyncInstances, Logger, limitedToInstance);

            DataSyncReplicaSet.Disable(limitedToInstance);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await DataSyncReplicaSet.Run(Query));
        }

        [Fact]
        public void Run_Command_LimitedToOneDisabledInstance_ExceptionThrown()
        {
            const int limitedToInstance = 2;
            DataSyncReplicaSet = new DataSyncReplicaSetLowLevel(ReplicaSetName, DataSyncInstances, Logger, limitedToInstance);

            DataSyncReplicaSet.Disable(limitedToInstance);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await DataSyncReplicaSet.Run(Command));
        }

        [Fact]
        public void Run_Command_ReplicaIsDisabled_ExceptionThrown()
        {
            const int instanceToDisable = 2;
            DataSyncReplicaSet.Disable(instanceToDisable);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await DataSyncReplicaSet.Run(Command));
        }

        [Fact]
        public void EnabledInstanceCount_ReplicaDisabled_CountAsExpected()
        {
            const int instanceToDisable = 2;
            DataSyncReplicaSet.Disable(instanceToDisable);
            int enabledInstances = DataSyncReplicaSet.EnabledInstanceCount();

            Assert.Equal(NumberOfGraphInstances-1, enabledInstances);
        }

        [Fact]
        public void EnabledInstanceCount_AllReplicaDisabled_CountIs0()
        {
            for (int instance = 0; instance < NumberOfGraphInstances; ++instance)
            {
                DataSyncReplicaSet.Disable(instance);
            }
            int enabledInstances = DataSyncReplicaSet.EnabledInstanceCount();

            Assert.Equal(0, enabledInstances);
        }
    }
}
#endif
