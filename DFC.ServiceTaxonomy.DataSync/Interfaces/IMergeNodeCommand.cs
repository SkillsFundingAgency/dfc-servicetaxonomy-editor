using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    public interface IMergeNodeCommand : ICommand
    {
        HashSet<string> NodeLabels { get; set; }

        string? IdPropertyName { get; set; }

        IDictionary<string, object> Properties { get; set; }
    }
}
