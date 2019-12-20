using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IMergeNodeCommand
    {
        void Initialise(string nodeLabel, string idPropertyName, IReadOnlyDictionary<string, object> propertyMap);
        Query Query { get; }
    }}
