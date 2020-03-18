using System.Collections.Generic;
using System.Text;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Queries
{
    public class GetNodesCypherQuery : IQuery<string>
    {
        public string MatchPropertyName { get; }
        public string MatchPropertyValue { get; }
        public string SourcePropertyName { get; }
        public string DestPropertyName { get; }
        public Dictionary<long, INode> Nodes { get; }
        public HashSet<IRelationship> Relationships { get; }
        public long SelectedNodeId { get; set; }

        public GetNodesCypherQuery(string matchPropertyName, string matchPropertyValue, string sourcePropertyName, string destPropertyName)
        {
            MatchPropertyName = matchPropertyName;
            MatchPropertyValue = matchPropertyValue;
            SourcePropertyName = sourcePropertyName;
            DestPropertyName = destPropertyName;

            Nodes = new Dictionary<long, INode>();
            Relationships = new HashSet<IRelationship>();
            SelectedNodeId = -1;
        }

        public List<string> ValidationErrors()
        {
            return new List<string>();
        }

        public Query Query
        {
            get
            {
                // $"match (n:ncs__JobProfile {{uri:\"{fetch}\"}})-[r]-(d) optional match (d)-[r1:esco__relatedEssentialSkill|:esco__relatedOptionalSkill|:ncs__hasUniversityLink|:ncs__hasUniversityRequirement]-(d1) return n, d, r, r1, d1";

                var cypherQuery = new StringBuilder();

                cypherQuery.Append($"MATCH (s {{{MatchPropertyName}:'{MatchPropertyValue}'}})");
                cypherQuery.Append($"OPTIONAL MATCH (s)-[r1]-(d1)");
                cypherQuery.Append("RETURN s,r1,d1");
                //cypherQuery.Append($"OPTIONAL MATCH (s)-[r1]-(d1)-[r2]-(d2)");
                //cypherQuery.Append("RETURN s,r1,d1,r2,d2");

                return new Query(cypherQuery.ToString());
            }
        }

        public string ProcessRecord(IRecord record)
        {
            var sourceNode = record["s"].As<INode>();
            var destNode = record["d1"].As<INode>();
            var relationship = record["r1"].As<IRelationship>();
            //var destNode2 = record["d2"].As<INode>();
            //var relationship2 = record["r2"].As<IRelationship>();

            Nodes[sourceNode.Id] = sourceNode;

            SelectedNodeId = sourceNode.Id;

            string sourceLabel = sourceNode.Properties.ContainsKey(SourcePropertyName) ? sourceNode.Properties[SourcePropertyName].ToString() : string.Empty;
            string result = sourceLabel;

            if (relationship != null)
            {
                Relationships.Add(relationship);
                result += $" - [{relationship.Type}]";
            }
            if (destNode != null)
            {
                Nodes[destNode.Id] = destNode;
                result += " - " + (destNode.Properties.ContainsKey(DestPropertyName) ? destNode.Properties[DestPropertyName].ToString() : string.Empty);
            }

            //if (relationship2 != null)
            //{
            //    Relationships.Add(relationship2);
            //    result += $" - [{relationship2.Type}]";
            //}
            //if (destNode2 != null)
            //{
            //    Nodes[destNode2.Id] = destNode2;
            //    result += " - " + (destNode2.Properties.ContainsKey(DestPropertyName) ? destNode2.Properties[DestPropertyName].ToString() : string.Empty);
            //}

            return result;
        }
    }
}
