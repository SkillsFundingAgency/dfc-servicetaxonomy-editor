using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Neo4j;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Commands
{
    public class MergeNodeCommandTests
    {
        public Neo4JMergeNodeCommand MergeNodeCommand;

        public MergeNodeCommandTests()
        {
            MergeNodeCommand = new Neo4JMergeNodeCommand
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
            /*var exception =*/ Assert.Throws<InvalidOperationException>(MergeNodeCommand.CheckIsValid);
        }

        [Fact]
        public void CheckIsValid_MissingNodeLabels_ThrowsInvalidOperationException()
        {
            MergeNodeCommand.NodeLabels = new HashSet<string>();
            /*var exception =*/ Assert.Throws<InvalidOperationException>(MergeNodeCommand.CheckIsValid);
        }

        [Fact]
        public void CheckIsValid_MergeNodeCommandValid_NoExceptionThrown()
        {
            MergeNodeCommand.CheckIsValid();
        }
    }
}
