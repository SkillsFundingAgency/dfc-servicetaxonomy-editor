using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Services
{
    public interface INeo4JToOwlGeneratorService
    {
        OwlDataModel CreateOwlDataModels(long selectedNodeId, Dictionary<long, INode> nodes, HashSet<IRelationship> relationships, string prefLabel);
    }
}
