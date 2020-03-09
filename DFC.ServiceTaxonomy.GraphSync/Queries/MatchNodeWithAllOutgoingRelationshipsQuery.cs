using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    public class MatchNodeWithAllOutgoingRelationshipsQuery : IQuery<IRecord>
    {
        private string ContentType { get; }
        private string Uri { get; }

        public MatchNodeWithAllOutgoingRelationshipsQuery(string contentType, string uri)
        {
            ContentType = contentType;
            Uri = uri;
        }

        public void CheckIsValid()
        {
            // nothing to check, all properties are non-nullable
        }

        public Query Query
        {
            get
            {
                return new Query($"match (s:ncs__{ContentType} {{ uri: '{Uri}' }})-[r]->(d) return s, r, d");
            }
        }

        public IRecord ProcessRecord(IRecord record)
        {
            return record;
        }
    }
}
