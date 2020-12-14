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
    }
}
