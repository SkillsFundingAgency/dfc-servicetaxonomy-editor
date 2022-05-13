using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphVisualiser.Models;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Configuration;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Services
{
    public class CosmosDbToOwlGeneratorService : OwlDataGeneratorService, IOwlGeneratorService
    {
        public CosmosDbToOwlGeneratorService(
            IOptionsMonitor<OwlDataGeneratorConfigModel> owlDataGeneratorConfigModel)
            : base(owlDataGeneratorConfigModel)
        {
        }

        public OwlDataModel CreateOwlDataModels(long? selectedNodeId, IEnumerable<INode> nodes, HashSet<IRelationship> relationships, string prefLabel)
        {
            TransformNodes(nodes, prefLabel);
            TransformRelationships(relationships);

            var result = new OwlDataModel
            {
                Namespace = CreateNamespaces(),
                Header = CreateHeader(),
                Settings = CreateSettings(),
                Class = nodeDataModels.Select(n => CreateClass(n, selectedNodeId?.ToString())).ToList(),
                ClassAttribute = nodeDataModels.Select(CreateClassAttribute).ToList(),
                Property = relationshipDataModels.Select(CreateProperty).ToList(),
                PropertyAttribute = relationshipDataModels.Select(CreatePropertyAttribute).ToList(),
            };

            return result;
        }

        private void TransformNodes(IEnumerable<INode> nodes, string prefLabel)
        {
            nodeDataModels = nodes
                .Select(node => new NodeDataModel(node, prefLabel))
                .ToList();
        }

        private void TransformRelationships(IEnumerable<IRelationship> relationships)
        {
            relationshipDataModels = relationships
                .Select(a => new RelationshipDataModel
                {
                    Id = $"{a.Id}",
                    Label = a.Type,
                    Domain = $"Class{a.StartNodeId}",
                    Range = $"Class{a.EndNodeId}"
                }).ToList();
        }
    }
}
