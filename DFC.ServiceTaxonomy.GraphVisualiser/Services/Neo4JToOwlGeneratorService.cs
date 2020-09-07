using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphVisualiser.Models;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Configuration;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Services
{
    public class Neo4JToOwlGeneratorService : OwlDataGeneratorService, INeo4JToOwlGeneratorService
    {
        private long minRelationshipId;

        public Neo4JToOwlGeneratorService(IOptionsMonitor<OwlDataGeneratorConfigModel> owlDataGeneratorConfigModel) : base(owlDataGeneratorConfigModel) { }

        public OwlDataModel CreateOwlDataModels(long selectedNodeId, IEnumerable<INode> nodes, HashSet<IRelationship> relationships, string prefLabel)
        {
            //minNodeId = nodes.Keys.Min() - 1;
            minRelationshipId = relationships.Count > 0 ? relationships.Min(r => r.Id) - 1 : 0;

            TransformData(nodes, prefLabel);
            TransformData(relationships);

            var result = new OwlDataModel
            {
                Namespace = CreateNamespaces(),
                Header = CreateHeader(),
                Settings = CreateSettings(),
                Class = nodeDataModels.Select(n => CreateClass(n, $"{selectedNodeId}")).ToList(),
                ClassAttribute = nodeDataModels.Select(CreateClassAttribute).ToList(),
                Property = relationshipDataModels.Select(CreateProperty).ToList(),
                PropertyAttribute = relationshipDataModels.Select(CreatePropertyAttribute).ToList(),
            };

            return result;
        }

        private void TransformData(IEnumerable<INode> nodes, string prefLabel)
        {
            nodeDataModels = (from a in nodes
                              select new NodeDataModel
                              {
                                  Id = $"Class{a.Id}",
                                  Key = 1,
                                  Type = a.Labels.First(l => l != "Resource" || l == "esco__Occupation" || l == "esco__Skill"),
                                  Label = GetPropertyValue(a, new[] { prefLabel, "Description", "FurtherInfo" }),
                                  Comment = GetPropertyValue(a, new[] { "Description" }),
                                  StaxProperties = a.Properties.Where(p => p.Key != prefLabel).Select(p => $"{p.Key}:{p.Value}").ToList()
                              }
            ).ToList();
        }

        private void TransformData(HashSet<IRelationship> relationships)
        {
            relationshipDataModels = (from a in relationships
                                      select new RelationshipDataModel
                                      {
                                          Id = $"{a.Id}",
                                          Label = a.Type,
                                          Domain = $"Class{a.StartNodeId}",
                                          Range = $"Class{a.EndNodeId}"
                                      }
            ).ToList();
        }

        private string GetPropertyValue(INode node, string[] names)
        {
            foreach (string name in names)
            {
                string result = node.Properties.ContainsKey(name)
                                ? (string)node.Properties[name]
                                : string.Empty;

                if (!string.IsNullOrWhiteSpace(result))
                {
                    return result;
                }
            }

            return string.Empty;
        }
    }
}
