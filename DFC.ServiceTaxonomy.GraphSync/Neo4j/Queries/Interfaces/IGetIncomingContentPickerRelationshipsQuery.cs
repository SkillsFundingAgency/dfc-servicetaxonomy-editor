using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces
{
    //don't have to worry about twoWay, as content picker doesn't support yet
    public interface IGetIncomingContentPickerRelationshipsQuery : IQuery<INodeWithOutgoingRelationships?>
    {
        IEnumerable<string> NodeLabels { get; set; }
        string? IdPropertyName { get; set; }
        object? IdPropertyValue { get; set; }
    }
}
