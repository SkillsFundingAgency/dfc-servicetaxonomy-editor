using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphVisualiser.Models;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Services
{
    public class Neo4JToOwlGeneratorService : OwlDataGeneratorService, INeo4JToOwlGeneratorService
    {
        private long minNodeId;
        private long minRelationshipId;

        public OwlDataModel CreateOwlDataModels(long selectedNodeId, Dictionary<long, INode> nodes, HashSet<IRelationship> relationships, string prefLabel)
        {
            minNodeId = nodes.Keys.Min() - 1;
            minRelationshipId = relationships.Count > 0 ? relationships.Min(r => r.Id) - 1 : 0;

            TransformData(nodes, prefLabel);
            TransformData(relationships);

            var result = new OwlDataModel
            {
                Namespace = CreateNamespaces(),
                Header = CreateHeader(),
                Settings = CreateSettings(),
                Class = nodeDataModels.Select(n => CreateClass(n, $"{selectedNodeId - minNodeId}")).ToList(),
                ClassAttribute = nodeDataModels.Select(n => CreateClassAttribute(n)).ToList(),
                Property = relationshipDataModels.Select(r => CreateProperty(r)).ToList(),
                PropertyAttribute = relationshipDataModels.Select(r => CreatePropertyAttribute(r)).ToList(),
            };

            return result;
        }

        private void TransformData(Dictionary<long, INode> nodes, string prefLabel)
        {
            nodeDataModels = (from a in nodes
                              select new NodeDataModel
                              {
                                  Id = $"Class{a.Key - minNodeId}",
                                  Key = a.Key,
                                  Type = a.Value.Labels.First(l => l.StartsWith("ncs__") || l == "esco__Occupation" || l == "esco__Skill"),
                                  Label = (string)a.Value.Properties[prefLabel],
                                  Comment = a.Value.Properties.ContainsKey("ncs__Description")
                                            ? (string)a.Value.Properties["ncs__Description"]
                                            : string.Empty,
                                  StaxProperties = a.Value.Properties.Where(p => p.Key != prefLabel).Select(p => $"{p.Key}:{p.Value}").ToList(),
                              }
            );
        }

        private void TransformData(HashSet<IRelationship> relationships)
        {
            relationshipDataModels = (from a in relationships
                                      select new RelationshipDataModel
                                      {
                                          Id = $"{a.Id - minRelationshipId}",
                                          Label = a.Type,
                                          Domain = $"Class{a.StartNodeId - minNodeId}",
                                          Range = $"Class{a.EndNodeId - minNodeId}"
                                      }
            );
        }

    }
}
