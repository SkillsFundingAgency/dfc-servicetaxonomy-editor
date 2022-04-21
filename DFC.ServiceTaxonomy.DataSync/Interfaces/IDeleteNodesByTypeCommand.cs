using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    public interface IDeleteNodesByTypeCommand : ICommand
    {
        HashSet<string> NodeLabels { get; set; }
    }
}
