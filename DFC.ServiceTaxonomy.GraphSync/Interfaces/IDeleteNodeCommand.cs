using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface IDeleteNodeCommand : ICommand
    {
        HashSet<string> NodeLabels { get; set; }

        string? IdPropertyName { get; set; }

        object? IdPropertyValue { get; set; }

        bool DeleteNode { get; set; }

        SyncOperation SyncOperation { get; set; }

        IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; set; }
    }
}
