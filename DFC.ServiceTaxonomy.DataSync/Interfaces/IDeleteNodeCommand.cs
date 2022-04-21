using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    public interface IDeleteNodeCommand : ICommand
    {
        HashSet<string> NodeLabels { get; set; }

        string? IdPropertyName { get; set; }

        object? IdPropertyValue { get; set; }

        bool DeleteNode { get; set; }

        IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; set; }
    }
}
