using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IMergeNodeCommand : ICommand
    {
        HashSet<string> NodeLabels { get; set; }
        string? IdPropertyName { get; set; }
        IDictionary<string, object> Properties { get; set; }
    }}
