using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataVisualiser.Models.Owl;

namespace DFC.ServiceTaxonomy.DataVisualiser.Services
{
    public interface IOwlGeneratorService
    {
        OwlDataModel CreateOwlDataModels(
            long? selectedNodeId,
            IEnumerable<INode> nodes,
            HashSet<IRelationship> relationships,
            string prefLabel);
    }
}
