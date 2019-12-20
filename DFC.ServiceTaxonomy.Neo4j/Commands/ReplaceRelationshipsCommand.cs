using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public class ReplaceRelationshipsCommand : IReplaceRelationshipsCommand
    {
        private Query? _query;

        public void Initialise(string sourceNodeLabel, string sourceIdPropertyName, string sourceIdPropertyValue,
            IReadOnlyDictionary<(string destNodeLabel,string destIdPropertyName,string relationshipType),IEnumerable<string>> relationships)
        {
            if (!relationships.Any()) // could return noop query instead
                throw new ArgumentException($"{nameof(Initialise)} called with no relationships");

            //todo: bi-directional relationships
            //todo: rewrite for elegance/perf. selectmany?
            const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
            var nodeMatchBuilder = new StringBuilder($"match (s:{sourceNodeLabel} {{{sourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
            var existingRelationshipsMatchBuilder = new StringBuilder();
            var mergeBuilder = new StringBuilder();
            var parameters = new Dictionary<string, object> {{sourceIdPropertyValueParamName, sourceIdPropertyValue}};
            int ordinal = 0;
            //todo: better name relationship=> relationships, relationships=>?

            var distinctRelationshipTypes = new Dictionary<string,string>();

            foreach (var relationship in relationships)
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

            var existingRelationshipVariablesString = string.Join(',',Enumerable.Range(1, ordinal).Select(o => $"r{o}"));

            _query = new Query($"{nodeMatchBuilder}\r\n{existingRelationshipsMatchBuilder}\r\ndelete {existingRelationshipVariablesString}\r\n{mergeBuilder}\r\nreturn s", parameters);
        }

        public Query Query => _query
                              ?? throw new InvalidOperationException("Command has not been initialised");

        public static implicit operator Query(ReplaceRelationshipsCommand c) => c.Query;
    }
}
