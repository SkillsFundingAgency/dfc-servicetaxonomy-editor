using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IDeleteNodeCommand : ICommand
    {
        HashSet<string> NodeLabels { get; set; }
        string? IdPropertyName { get; set; }
        object? IdPropertyValue { get; set; }
        public bool DeleteNode { get; set; }
    }
}
