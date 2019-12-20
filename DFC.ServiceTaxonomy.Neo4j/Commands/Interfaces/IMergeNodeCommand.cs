using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IMergeNodeCommand
    {
        string? NodeLabel { get; set; }
        string? IdPropertyName { get; set; }
        IDictionary<string, object> Properties { get; set; }

        Query Query { get; }
    }}
