using System.Collections.Generic;
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

        public List<string> ValidationErrors()
        {
            return new List<string>();
        }

        public Query Query
        {
            get
            {
                //todo: remove hardcoding
                return new Query($"match (s:ncs__{ContentType} {{ uri: '{Uri}' }}) optional match (s)-[r]->(d) return s, r, d");
            }
        }

        public IRecord ProcessRecord(IRecord record)
        {
            return record;
        }
    }
}
