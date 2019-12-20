using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    //todo: allow incremental build up, i.e. AddProperty etc.
    //todo: inject database and add execute?
    //BUG: adding new namespace prefix doesn't add it to global list
    public class MergeNodeCommand : IMergeNodeCommand
    {
        private Query? _query;

        public Query Initialise(string nodeLabel, string idPropertyName, IReadOnlyDictionary<string, object> propertyMap)
        {
            return _query = new Query(
                $"MERGE (n:{nodeLabel} {{ {idPropertyName}:'{propertyMap[idPropertyName]}' }}) SET n=$properties RETURN ID(n)",
                new Dictionary<string,object> {{"properties", propertyMap}});
        }

        public Query Query => _query
                              ?? throw new InvalidOperationException("Command has not been initialised");

        public static implicit operator Query(MergeNodeCommand c) => c.Query;
    }
}
