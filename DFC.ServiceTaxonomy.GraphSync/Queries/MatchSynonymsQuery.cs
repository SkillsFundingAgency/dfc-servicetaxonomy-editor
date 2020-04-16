using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;
using DFC.ServiceTaxonomy.Neo4j.Queries;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    public class MatchSynonymsQuery : IQuery<IRecord>
    {
        private string FirstNodeLabel { get; }
        private string SecondNodeLabel { get; }
        private string PropertyValue { get; }

        private string[] RelationshipTypes { get; }

        public MatchSynonymsQuery(string firstNodeLabel, string secondNodeLabel, string propertyValue, params string[] relationshipTypes)
        {
            FirstNodeLabel = firstNodeLabel;
            SecondNodeLabel = secondNodeLabel;
            PropertyValue = propertyValue;
            RelationshipTypes = relationshipTypes;
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(FirstNodeLabel))
            {
                validationErrors.Add($"FirstNodeLabel not provided.");
            }

            if (string.IsNullOrWhiteSpace(SecondNodeLabel))
            {
                validationErrors.Add($"SecondNodeLabel not provided.");
            }

            if (string.IsNullOrWhiteSpace(PropertyValue))
            {
                validationErrors.Add($"PropertyValue not provided.");
            }

            if(RelationshipTypes.Length == 0)
            {
                validationErrors.Add($"At least one RelationshipType must be provided.");
            }

            return validationErrors;
        }

        public Query Query
        {  
            get
            {
                this.CheckIsValid();
                return new Query($"MATCH(o:{FirstNodeLabel})-[:{string.Join("|:", RelationshipTypes)}]- (l:{SecondNodeLabel}) WITH {{ label: o.{PropertyValue}, values: REDUCE(result = o.{PropertyValue}, s in collect(l.{PropertyValue}) | result + \",\" + s) }} AS synonyms WITH {{ Results: collect(synonyms.values)}} as results RETURN results");
            }
        }

        public IRecord ProcessRecord(IRecord record)
        {
            return record;
        }
    }
}

