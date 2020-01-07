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

    public class ReplaceRelationshipsCommand : IReplaceRelationshipsCommand
    {
        public string? SourceNodeLabel { get; set; }
        public string? SourceIdPropertyName { get; set; }
        public string? SourceIdPropertyValue { get; set; }
        public IDictionary<(string destNodeLabel,string destIdPropertyName,string relationshipType), IEnumerable<string>> Relationships {  get; set; }
            = new Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>>();

        //todo: make dictionary private, and add AddRelationship()?
        public void AddRelationshipsTo(string relationshipType, string destNodeLabel, string destIdPropertyName, params string[] destIdPropertyValues)
        {
            Relationships.Add((destNodeLabel, destIdPropertyName,  relationshipType), destIdPropertyValues);
        }

        private Query CreateQuery()
        {
            if (SourceNodeLabel == null)
                throw new InvalidOperationException($"{nameof(SourceNodeLabel)} not supplied");

            if (SourceIdPropertyName == null)
                throw new InvalidOperationException($"{nameof(SourceIdPropertyName)} not supplied");

            if (SourceIdPropertyValue == null)
                throw new InvalidOperationException($"{nameof(SourceIdPropertyValue)} not supplied");

            //todo: bi-directional relationships
            //todo: rewrite for elegance/perf. selectmany?
            const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
            var nodeMatchBuilder = new StringBuilder($"match (s:{SourceNodeLabel} {{{SourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
            var existingRelationshipsMatchBuilder = new StringBuilder();
            var mergeBuilder = new StringBuilder();
            var parameters = new Dictionary<string, object> {{sourceIdPropertyValueParamName, SourceIdPropertyValue}};
            int ordinal = 0;
            //todo: better name relationship=> relationships, relationships=>?

            var distinctRelationshipTypes = new Dictionary<string,string>();

            foreach (var relationship in Relationships)
            {
                distinctRelationshipTypes[relationship.Key.relationshipType] = relationship.Key.destNodeLabel;

                foreach (string destIdPropertyValue in relationship.Value)
                {
                    string destNodeVariable = $"d{++ordinal}";
                    string destIdPropertyValueParamName = $"{destNodeVariable}Value";

                    nodeMatchBuilder.Append($"\r\nmatch ({destNodeVariable}:{relationship.Key.destNodeLabel} {{{relationship.Key.destIdPropertyName}:${destIdPropertyValueParamName}}})");
                    parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);

                    mergeBuilder.Append($"\r\nmerge (s)-[:{relationship.Key.relationshipType}]->({destNodeVariable})");
                }
            }

            ordinal = 0;
            foreach (var relationshipTypeToDestNodeLabel in distinctRelationshipTypes)
            {
                existingRelationshipsMatchBuilder.Append($"\r\noptional match (s)-[r{++ordinal}:{relationshipTypeToDestNodeLabel.Key}]->(:{relationshipTypeToDestNodeLabel.Value})");
            }

            string existingRelationshipVariablesString = string.Join(',',Enumerable.Range(1, ordinal).Select(o => $"r{o}"));

            return new Query($"{nodeMatchBuilder}\r\n{existingRelationshipsMatchBuilder}\r\ndelete {existingRelationshipVariablesString}\r\n{mergeBuilder}\r\nreturn s", parameters);
        }

        public Query Query => CreateQuery();

        public static implicit operator Query(ReplaceRelationshipsCommand c) => c.Query;
    }
}
