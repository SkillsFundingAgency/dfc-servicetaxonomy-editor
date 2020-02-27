using System;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    //todo: inject database and add execute?
    public class DeleteNodeCommand : IDeleteNodeCommand
    {
        public string ContentType { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;

        private Query CreateQuery()
        {
            if (string.IsNullOrWhiteSpace(ContentType))
                throw new InvalidOperationException($"{nameof(ContentType)} is null");

            if (string.IsNullOrWhiteSpace(Uri))
                throw new InvalidOperationException($"{nameof(Uri)} is null");

            return new Query($"MATCH (n:ncs__{ContentType} {{ uri:'{Uri}' }})-[r]->() DELETE n, r");
        }

        public Query Query => CreateQuery();

        public static implicit operator Query(DeleteNodeCommand c) => c.Query;
    }
}
