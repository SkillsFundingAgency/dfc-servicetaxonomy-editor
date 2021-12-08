using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries
{
    public class Neo4JMatchSynonymsQuery : IQuery<IRecord>
    {
        private string FirstNodeLabel { get; }
        private string SecondNodeLabel { get; }
        private string PropertyValue { get; }

        private string[] RelationshipTypes { get; }

        public Neo4JMatchSynonymsQuery(string firstNodeLabel, string secondNodeLabel, string propertyValue, params string[] relationshipTypes)
        {
            FirstNodeLabel = firstNodeLabel;
            SecondNodeLabel = secondNodeLabel;
            PropertyValue = propertyValue;
            RelationshipTypes = relationshipTypes;
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if(RelationshipTypes.Length == 0)
            {
                validationErrors.Add("At least one RelationshipType must be provided.");
            }

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();
                return new Query($"MATCH(o:{FirstNodeLabel})-[:{string.Join("|", RelationshipTypes)}]- (l:{SecondNodeLabel}) WITH {{ label: o.{PropertyValue}, values: REDUCE(result = o.{PropertyValue}, s in collect(l.{PropertyValue}) | result + \",\" + s) }} AS synonyms WITH {{ Results: collect(synonyms.values)}} as results RETURN results");
            }
        }

        public IRecord ProcessRecord(IRecord record)
        {
            return record;
        }
    }
}

