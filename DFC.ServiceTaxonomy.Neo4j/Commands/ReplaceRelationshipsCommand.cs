using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    //todo: now we delete all relationships, we should be able to just create rather than merge. change once we have integration test coverage
    //todo: refactor to only create relationships to one dest node label?

    // relationshiptocreate? as opposed to the driver's returned relationship
    //todo: interface?
    public class Relationship
    {
        //source properties belong in here really, unless rename class

        public Relationship(string relationshipType, IDictionary<string, object>? properties, IEnumerable<string> destinationNodeLabels, string destinationNodeIdPropertyName, IEnumerable<string> destinationNodeIdPropertyValues)
        {
            RelationshipType = relationshipType;
            Properties = properties;
            DestinationNodeLabels = destinationNodeLabels;
            DestinationNodeIdPropertyName = destinationNodeIdPropertyName;
            DestinationNodeIdPropertyValues = destinationNodeIdPropertyValues;
        }

        public string RelationshipType { get; }    // RelationshipType, not type to differentiate from System.Type

        public IDictionary<string, object>? Properties { get; }

        public IEnumerable<string> DestinationNodeLabels { get; }

        public string DestinationNodeIdPropertyName { get; }
        public IEnumerable<string> DestinationNodeIdPropertyValues { get; }
    }

    public class ReplaceRelationshipsCommand : IReplaceRelationshipsCommand
    {
        //todo: ctor with source properties?

        public HashSet<string> SourceNodeLabels { get; set; } = new HashSet<string>();
        public string? SourceIdPropertyName { get; set; }
        public string? SourceIdPropertyValue { get; set; }
        // public IDictionary<(string destNodeLabel,string destIdPropertyName,string relationshipType), IEnumerable<string>> Relationships {  get; set; }
        //     = new Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>>();

        public IEnumerable<Relationship> Relationships
        {
            get { return RelationshipsList; }
        }

        private List<Relationship> RelationshipsList { get; set; } = new List<Relationship>();

        public void AddRelationshipsTo(string relationshipType, IEnumerable<string> destNodeLabels, string destIdPropertyName, params string[] destIdPropertyValues)
        {
            RelationshipsList.Add(new Relationship(relationshipType, null, destNodeLabels, destIdPropertyName, destIdPropertyValues));
        }

        private Query CreateQuery()
        {
            if (SourceNodeLabels == null)
                throw new InvalidOperationException($"{nameof(SourceNodeLabels)} is null");

            if (!SourceNodeLabels.Any())
                throw new InvalidOperationException("No source labels");

            if (SourceIdPropertyName == null)
                throw new InvalidOperationException($"{nameof(SourceIdPropertyName)} is null");

            if (SourceIdPropertyValue == null)
                throw new InvalidOperationException($"{nameof(SourceIdPropertyValue)} is null");

            //todo: bi-directional relationships
            //todo: rewrite for elegance/perf. selectmany?
            const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
            var nodeMatchBuilder = new StringBuilder($"match (s:{string.Join(':', SourceNodeLabels)} {{{SourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
            var existingRelationshipsMatchBuilder = new StringBuilder();
            var mergeBuilder = new StringBuilder();
            var parameters = new Dictionary<string, object> {{sourceIdPropertyValueParamName, SourceIdPropertyValue}};
            int ordinal = 0;
            //todo: better name relationship=> relationships, relationships=>?

            var distinctRelationshipTypeToDestNode = new HashSet<(string type,string labels)>();

            foreach (var relationship in RelationshipsList)
            {
                string destNodeLabels = string.Join(':', relationship.DestinationNodeLabels.OrderBy(l => l));
                // different types could have different dest node labels
                // add unit/integration tests for this ^^ scenario
                distinctRelationshipTypeToDestNode.Add((relationship.RelationshipType, destNodeLabels));

                foreach (string destIdPropertyValue in relationship.DestinationNodeIdPropertyValues)
                {
                    string destNodeVariable = $"d{++ordinal}";
                    string destIdPropertyValueParamName = $"{destNodeVariable}Value";

                    nodeMatchBuilder.Append($"\r\nmatch ({destNodeVariable}:{destNodeLabels} {{{relationship.DestinationNodeIdPropertyName}:${destIdPropertyValueParamName}}})");
                    parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);

                    mergeBuilder.Append($"\r\nmerge (s)-[:{relationship.RelationshipType}]->({destNodeVariable})");
                }
            }

            ordinal = 0;
            foreach (var relationshipTypeToDestNode in distinctRelationshipTypeToDestNode)
            {
                existingRelationshipsMatchBuilder.Append($"\r\noptional match (s)-[r{++ordinal}:{relationshipTypeToDestNode.type}]->(:{relationshipTypeToDestNode.labels})");
            }

            string existingRelationshipVariablesString = string.Join(',',Enumerable.Range(1, ordinal).Select(o => $"r{o}"));

            return new Query($"{nodeMatchBuilder}\r\n{existingRelationshipsMatchBuilder}\r\ndelete {existingRelationshipVariablesString}\r\n{mergeBuilder}\r\nreturn s", parameters);
        }

        public Query Query => CreateQuery();

        public static implicit operator Query(ReplaceRelationshipsCommand c) => c.Query;
    }
}
