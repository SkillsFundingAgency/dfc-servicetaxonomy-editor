using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries
{
    //don't have to worry about twoWay, as content picker doesn't support yet
    public interface IGetIncomingContentPickerRelationshipsQuery : IQuery<INodeWithOutgoingRelationships?>
    {
        IEnumerable<string> NodeLabels { get; set; }
        string? IdPropertyName { get; set; }
        object? IdPropertyValue { get; set; }
    }
}
