using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Commands.Model;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Commands.Model
{
    public class CommandRelationshipTests
    {
        //todo: more unit tests
        [Fact]
        public void Equals_Same_ReturnsTrue()
        {
            var relationship1 = new CommandRelationship(
                "outgoingRelationshipType",
                "incomingRelationshipType",
                new[] {new KeyValuePair<string, object>("K", "V")},
                new[] {"L1", "L2"},
                "destinationNodeIdPropertyName",
                    new object[] {"ID1", "ID2"});

            var relationship2 = new CommandRelationship(
                "outgoingRelationshipType",
                "incomingRelationshipType",
                new[] {new KeyValuePair<string, object>("K", "V")},
                new[] {"L1", "L2"},
                "destinationNodeIdPropertyName",
                new object[] {"ID1", "ID2"});

            bool equals = relationship1.Equals(relationship2);

            Assert.True(equals);
        }
    }
}
