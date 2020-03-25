using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphLookup.Queries
{
    //todo: should be generic
    //public class GetPropertyOnNodeQuery<TValue> : IQuery<TValue>
    public class GetPropertyOnNodeQuery : IQuery<string?>
    {
        //todo: should be NodeLabels
        public string NodeLabel { get; }
        public string MatchPropertyName { get; }
        public string MatchPropertyValue { get; }
        public string PropertyName { get; }

        private const string PropertyValueVariableName = "v";

        public GetPropertyOnNodeQuery(string nodeLabel, string matchPropertyName, string matchPropertyValue, string propertyName)
        {
            NodeLabel = nodeLabel;
            MatchPropertyName = matchPropertyName;
            MatchPropertyValue = matchPropertyValue;
            PropertyName = propertyName;
        }

        public List<string> ValidationErrors()
        {
            return new List<string>();
        }

        public Query Query
        {
            get
            {
                return new Query($"match (n:{NodeLabel} {{{MatchPropertyName}:'{MatchPropertyValue}'}}) return n.{PropertyName} as {PropertyValueVariableName}");
            }
        }

        public string? ProcessRecord(IRecord record)
        {
            return record[PropertyValueVariableName].ToString();
        }
    }
}
