using System;
using System.Collections.Generic;
using System.Linq;
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
            if (!relationships.Any()) // could return noop statement instead
                throw new ArgumentException($"{nameof(MergeRelationships)} called with no relationships");

            //todo: for same task for create/edit, first delete all given relationships between source and dest nodes

            // match (n:ncs__JobProfile {uri:'https://nationalcareers.service.gov.uk/JobProfile/111111111'})
            // match (s:ncs__Skill {uri:'https://nationalcareers.service.gov.uk/Skill/111111111'})
            // match (q:ncs__QCFLevel {uri:'https://nationalcareers.service.gov.uk/QCFLevel/1111111111111'})
            // optional match (n)-[r2:ncs__test2]->(:ncs__QCFLevel)
            // optional match (s)-[r1:ncs__test]->(:ncs__JobProfile)
            // delete r1, r2
            // merge (n)-[:ncs__test2]->(q)
            //     merge (s)-[:ncs__test]->(n)
            // return s, n, q

            //todo: bi-directional relationships
            //todo: rewrite for elegance/perf. selectmany?
            const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
            var nodeMatchBuilder = new StringBuilder($"match (s:{sourceNodeLabel} {{{sourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
            var existingRelationshipsMatchBuilder = new StringBuilder();
            var mergeBuilder = new StringBuilder();
            var parameters = new Dictionary<string, object> {{sourceIdPropertyValueParamName, sourceIdPropertyValue}};
            // we could instead just use the final value of ordinal for this...
            // var existingRelationshipVariables = new List<string>();
            int ordinal = 0;
            //todo: better name relationship=> relationships, relationships=>?

            var distinctRelationshipTypes = new Dictionary<string,string>();

            foreach (var relationship in relationships)
            {
                foreach (string destIdPropertyValue in relationship.Value)
                {
                    string destNodeVariable = $"d{++ordinal}";
                    string destIdPropertyValueParamName = $"{destNodeVariable}Value";
                    // string existingRelationshipsVariable = $"r{++ordinal}";
                    // existingRelationshipVariables.Add(existingRelationshipsVariable);

                    nodeMatchBuilder.Append($"\r\nmatch ({destNodeVariable}:{relationship.Key.destNodeLabel} {{{relationship.Key.destIdPropertyName}:${destIdPropertyValueParamName}}})");
                    parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);

                    //todo: distinct relationship.Key.relationshipType
                    //existingRelationshipsMatchBuilder.Append($"\r\noptional match (s)-[:{relationship.Key.relationshipType}]->(:{relationship.Key.destNodeLabel})");
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
            //var existingRelationshipVariablesString = string.Join(',', existingRelationshipVariables);

            return new Statement($"{nodeMatchBuilder}\r\n{existingRelationshipsMatchBuilder}\r\ndelete {existingRelationshipVariablesString}\r\n{mergeBuilder}\r\nreturn s", parameters);
        }
    }
}
