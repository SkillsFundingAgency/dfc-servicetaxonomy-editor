using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Services
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
