using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Commands
{
    public class ReplaceRelationshipsCommandTests
    {
        public ReplaceRelationshipsCommand ReplaceRelationshipsCommand;

        public ReplaceRelationshipsCommandTests()
        {
            ReplaceRelationshipsCommand = new ReplaceRelationshipsCommand
            {
                SourceIdPropertyName = $"{nameof(ReplaceRelationshipsCommand.SourceIdPropertyName)}",
                SourceIdPropertyValue = $"{nameof(ReplaceRelationshipsCommand.SourceIdPropertyValue)}",
                SourceNodeLabels = {"SourceNodeLabel"}
            };
        }

        private void AddValidRelationshipTo() =>
            ReplaceRelationshipsCommand.AddRelationshipsTo(
                "relationshipType", null,
                new[] {"destNodeLabel"},
                "destIdPropertyName",
                "destIdPropertyValues");

        [Fact]
        public void CheckIsValid_NullSourceIdPropertyName_ThrowsInvalidOperationException()
        {
            AddValidRelationshipTo();

            ReplaceRelationshipsCommand.SourceIdPropertyName = null;
            /*var exception =*/ Assert.Throws<InvalidOperationException>(ReplaceRelationshipsCommand.CheckIsValid);
        }

        [Fact]
        public void CheckIsValid_NullSourceIdPropertyValue_ThrowsInvalidOperationException()
        {
            AddValidRelationshipTo();

            ReplaceRelationshipsCommand.SourceIdPropertyValue = null;
            /*var exception =*/ Assert.Throws<InvalidOperationException>(ReplaceRelationshipsCommand.CheckIsValid);
        }

        [Fact]
        public void CheckIsValid_MissingSourceNodeLabels_ThrowsInvalidOperationException()
        {
            AddValidRelationshipTo();

            ReplaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>();
            /*var exception =*/ Assert.Throws<InvalidOperationException>(ReplaceRelationshipsCommand.CheckIsValid);
        }

        [Fact]
        public void CheckIsValid_NoRelationshipTo_NoExceptionThrown()
        {
            ReplaceRelationshipsCommand.CheckIsValid();
        }

        [Fact]
        public void CheckIsValid_RelationshipToHasEmptyDestDestNodeLabels_ThrowsInvalidOperationException()
        {
            ReplaceRelationshipsCommand.AddRelationshipsTo("relationshipType", null, Enumerable.Empty<string>(), "destIdPropertyName");
            /*var exception =*/ Assert.Throws<InvalidOperationException>(ReplaceRelationshipsCommand.CheckIsValid);
        }

        [Fact]
        public void CheckIsValid_RelationshipToHasEmptyDestIdPropertyValues_NoExceptionThrown()
        {
            ReplaceRelationshipsCommand.AddRelationshipsTo("relationshipType", null, new[] {"destNodeLabels"}, "destIdPropertyName");
            ReplaceRelationshipsCommand.CheckIsValid();
        }

        [Fact]
        public void CheckIsValid_ReplaceRelationshipsCommandValid_NoExceptionThrown()
        {
            ReplaceRelationshipsCommand.CheckIsValid();
        }
    }
}
