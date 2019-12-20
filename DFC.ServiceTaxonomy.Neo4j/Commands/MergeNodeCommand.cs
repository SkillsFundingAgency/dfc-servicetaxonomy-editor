using System;
using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public interface IMergeNodeCommand
    {
        void Initialise(string nodeLabel, string idPropertyName, IReadOnlyDictionary<string, object> propertyMap);
        Query Query { get; }
    }

    //todo: allow incremental build up, i.e. AddProperty etc.
    public class MergeNodeCommand : IMergeNodeCommand
    {
        private Query? _query;

        public void Initialise(string nodeLabel, string idPropertyName, IReadOnlyDictionary<string, object> propertyMap)
        {
            _query = new Query(
                $"MERGE (n:{nodeLabel} {{ {idPropertyName}:'{propertyMap[idPropertyName]}' }}) SET n=$properties RETURN ID(n)",
                new Dictionary<string,object> {{"properties", propertyMap}});
        }

        public Query Query => _query
                              ?? throw new InvalidOperationException("Command has not been initialised");

        public static implicit operator Query(MergeNodeCommand c) => c.Query;
    }
}
