using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IMergeNodeCommand
    {
        HashSet<string> NodeLabels { get; set; }
        string? IdPropertyName { get; set; }
        IDictionary<string, object> Properties { get; set; }

        void CheckIsValid();

        Query Query { get; }
    }}
