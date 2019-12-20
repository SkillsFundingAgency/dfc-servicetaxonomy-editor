using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    //todo: inject database and add execute?
    //BUG: adding new namespace prefix doesn't add it to global list
    public class MergeNodeCommand : IMergeNodeCommand
    {
        public string? NodeLabel { get; set; }
        public string? IdPropertyName { get; set; }
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        private Query CreateQuery()
        {
            if (NodeLabel == null)
                throw new InvalidOperationException($"{nameof(NodeLabel)} not supplied");

            if (IdPropertyName == null)
                throw new InvalidOperationException($"{nameof(IdPropertyName)} not supplied");

            return new Query(
                $"MERGE (n:{NodeLabel} {{ {IdPropertyName}:'{Properties[IdPropertyName]}' }}) SET n=$properties RETURN ID(n)",
                new Dictionary<string,object> {{"properties", Properties}});
        }

        public Query Query => CreateQuery();

        public static implicit operator Query(MergeNodeCommand c) => c.Query;
    }
}
