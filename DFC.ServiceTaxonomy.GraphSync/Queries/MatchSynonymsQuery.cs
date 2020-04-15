using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    public class MatchSynonymsQuery : IQuery<IRecord>
    {
        private string FirstNodeLabel { get; }
        private string SecondNodeLabel { get; }
        private string PropertyValue { get; }

        private string[] RelationshipLabels { get; }

        public MatchSynonymsQuery(string firstNodeLabel, string secondNodeLabel, string propertyValue, params string[] relationshipLabels)
        {
            FirstNodeLabel = firstNodeLabel;
            SecondNodeLabel = secondNodeLabel;
            PropertyValue = propertyValue;
            RelationshipLabels = relationshipLabels;
        }

        public List<string> ValidationErrors()
        {
            return new List<string>();
        }

        public Query Query
        {
            get
            {
                return new Query($"MATCH(o:{FirstNodeLabel})-[:{string.Join("|:", RelationshipLabels)}]- (l:{SecondNodeLabel}) WITH {{ label: o.{PropertyValue}, values: REDUCE(result = o.{PropertyValue}, s in collect(l.{PropertyValue}) | result + \",\" + s) }} AS synonyms WITH {{ Results: collect(synonyms.values)}} as results RETURN results");
            }
        }

        public IRecord ProcessRecord(IRecord record)
        {
            return record;
        }
    }
}

