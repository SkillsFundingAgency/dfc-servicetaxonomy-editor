using System.Collections.Generic;
using System.Text;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Editor.Module.Neo4j.Generators
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
            //todo: for same task for create/edit, first delete all given relationships between source and dest nodes
            //todo: bi-directional relationships
            //todo: rewrite for elegance/perf. selectmany?
            const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
            var matchBuilder = new StringBuilder($"match (s:{sourceNodeLabel} {{{sourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
            var mergeBuilder = new StringBuilder();
            var parameters = new Dictionary<string, object> {{sourceIdPropertyValueParamName, sourceIdPropertyValue}};
            int destOrdinal = 0;
            //todo: better name relationship=> relationships, relationships=>?
            foreach (var relationship in relationships)
            {
                foreach (string destIdPropertyValue in relationship.Value)
                {
                    string destNodeVariable = $"d{++destOrdinal}";
                    string destIdPropertyValueParamName = $"{destNodeVariable}Value";
                    matchBuilder.Append($", ({destNodeVariable}:{relationship.Key.destNodeLabel} {{{relationship.Key.destIdPropertyName}:${destIdPropertyValueParamName}}})");
                    parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);
                    mergeBuilder.Append($"\r\nmerge (s)-[:{relationship.Key.relationshipType}]->({destNodeVariable})");
                }
            }

            return new Statement($"{matchBuilder}\r\n{mergeBuilder} return s", parameters);
        }
    }
}
