using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Commands
{
    public class MergeNodeCommandTests
    {
        public MergeNodeCommand MergeNodeCommand;

        public MergeNodeCommandTests()
        {
            MergeNodeCommand = new MergeNodeCommand
            {
                IdPropertyName = $"{nameof(MergeNodeCommand.IdPropertyName)}",
                NodeLabels = {"NodeLabel"},
                Properties = new Dictionary<string, object>()
            };
        }

        //Func<Query> act = () => MergeNodeCommand.Query;

        [Fact]
        public void CheckIsValid_NullIdPropertyName_ThrowsInvalidOperationException()
        {
            MergeNodeCommand.IdPropertyName = null;
            var exception = Assert.Throws<InvalidOperationException>(MergeNodeCommand.CheckIsValid);
        }

        [Fact]
        public void CheckIsValid_MissingNodeLabels_ThrowsInvalidOperationException()
        {
            MergeNodeCommand.NodeLabels = new HashSet<string>();
            var exception = Assert.Throws<InvalidOperationException>(MergeNodeCommand.CheckIsValid);
        }

        [Fact]
        public void CheckIsValid_MergeNodeCommandValid_NoExceptionThrown()
        {
            MergeNodeCommand.CheckIsValid();
        }
    }
}
