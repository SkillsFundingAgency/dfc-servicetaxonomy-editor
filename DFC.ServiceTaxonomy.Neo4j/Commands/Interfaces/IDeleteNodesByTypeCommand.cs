using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IDeleteNodesByTypeCommand : ICommand
    {
        HashSet<string> NodeLabels { get; set; }
    }
}
