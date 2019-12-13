using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Generators
{
    public static class StatementGenerator
    {
        public static Statement MergeNodes(string nodeLabel, IDictionary<string,object> propertyMap, string idPropertyName = "uri")
        {
            return new Statement(
                $"MERGE (n:{nodeLabel} {{ {idPropertyName}:'{propertyMap[idPropertyName]}' }}) SET n=$properties RETURN n",
                new Dictionary<string,object> {{"properties", propertyMap}});
        }

        public static Statement MergeRelationships(string sourceNodeLabel, string sourceIdPropertyName, string sourceIdPropertyValue,
            IDictionary<(string destNodeLabel,string destIdPropertyName,string relationshipType),IEnumerable<string>> relationships)
        {
            if (!relationships.Any()) // could return noop statement instead
                throw new ArgumentException($"{nameof(MergeRelationships)} called with no relationships");

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
                foreach (string destIdPropertyValue in relationship.Value)
                {
                    string destNodeVariable = $"d{++ordinal}";
                    string destIdPropertyValueParamName = $"{destNodeVariable}Value";

                    nodeMatchBuilder.Append($"\r\nmatch ({destNodeVariable}:{relationship.Key.destNodeLabel} {{{relationship.Key.destIdPropertyName}:${destIdPropertyValueParamName}}})");
                    parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);

                    distinctRelationshipTypes[relationship.Key.relationshipType] = relationship.Key.destNodeLabel;

                    mergeBuilder.Append($"\r\nmerge (s)-[:{relationship.Key.relationshipType}]->({destNodeVariable})");
                }
            }

            ordinal = 0;
            foreach (var relationshipTypeToDestNodeLabel in distinctRelationshipTypes)
            {
                existingRelationshipsMatchBuilder.Append($"\r\noptional match (s)-[r{++ordinal}:{relationshipTypeToDestNodeLabel.Key}]->(:{relationshipTypeToDestNodeLabel.Value})");
            }

            var existingRelationshipVariablesString = string.Join(',',Enumerable.Range(1, ordinal).Select(o => $"r{o}"));


            return new Statement($"{nodeMatchBuilder}\r\n{existingRelationshipsMatchBuilder}\r\ndelete {existingRelationshipVariablesString}\r\n{mergeBuilder}\r\nreturn s", parameters);
        }
    }
}
