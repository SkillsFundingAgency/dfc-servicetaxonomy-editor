using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphLookup.CosmosDb.Queries
{
    //todo: should be generic
    //public class GetPropertyOnNodeQuery<TValue> : IQuery<TValue>
    public class CosmosDbGetPropertyOnNodeQuery : IQuery<string?>
    {
        //todo: should be NodeLabels
        public string NodeLabel { get; }
        public string MatchPropertyName { get; }
        public string MatchPropertyValue { get; }
        public string PropertyName { get; }

        private const string PropertyValueVariableName = "v";

        public CosmosDbGetPropertyOnNodeQuery(string nodeLabel, string matchPropertyName, string matchPropertyValue, string propertyName)
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
