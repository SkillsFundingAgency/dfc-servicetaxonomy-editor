using System;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public class DeleteNodeCommand : IDeleteNodeCommand
    {
        public string? ContentType { get; set; }
        public string? Uri { get; set; }

        private Query CreateQuery()
        {
            if (ContentType == null)
                throw new InvalidOperationException($"{nameof(ContentType)} is null");

            if (Uri == null)
                throw new InvalidOperationException($"{nameof(Uri)} is null");

            return new Query($"MATCH (n:ncs__{ContentType} {{ uri:'{Uri}' }})-[r]->() DELETE n, r");
        }

        public Query Query => CreateQuery();

        public static implicit operator Query(DeleteNodeCommand c) => c.Query;
    }
}
