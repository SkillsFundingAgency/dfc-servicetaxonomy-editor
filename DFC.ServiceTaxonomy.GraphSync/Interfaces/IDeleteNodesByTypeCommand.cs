using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface IDeleteNodesByTypeCommand : ICommand
    {
        HashSet<string> NodeLabels { get; set; }
    }
}
