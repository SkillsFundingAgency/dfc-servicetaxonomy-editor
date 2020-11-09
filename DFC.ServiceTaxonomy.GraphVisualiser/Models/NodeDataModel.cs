using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models
{
    public class NodeDataModel
    {
        public string? Id { get; set; }
        public long Key { get; set; }
        public string? Type { get; set; }
        public string? Label { get; set; }
        public string? Comment { get; set; }
        public List<string> StaxProperties { get; set; }
        public string? NodeId { get; set; }

        public NodeDataModel(INode node, string prefLabel, ISyncNameProvider syncNameProvider)
        {
            //todo: we've already calculated the label earlier, so we don't need this logic (if we make it available here)
            //todo: don't hardcode preexisting nodes
            string type = node.Labels.FirstOrDefault(l => l == "esco__Occupation" || l == "esco__Skill") ??
                          node.Labels.First(l => l != "Resource");

            const string escoPrefix = "esco__";
            if (type.StartsWith(escoPrefix))
                type = type.Substring(escoPrefix.Length);

            Id = $"Class{node.Id}";
            Key = 1;
            Type = type;
            Label = GetPropertyValue(node, new[] {prefLabel, "Description", "FurtherInfo"});
            Comment = GetPropertyValue(node, new[] {"Description"});
            StaxProperties = node.Properties
                .Where(p => p.Key != prefLabel)
                .Select(p => $"{p.Key}:{p.Value}")
                .ToList();
            NodeId = GetNodeId(node.Properties, type, syncNameProvider);
        }

        public NodeDataModel(string? id, long key, string? type, string? label, string? comment)
        {
            Id = id;
            Key = key;
            Type = type;
            Label = label;
            Comment = comment;
            StaxProperties = new List<string>();
        }

        private string GetPropertyValue(INode node, IEnumerable<string> names)
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

        private static string? GetNodeId(
            IReadOnlyDictionary<string, object> staxProperties,
            string? contentType,
            ISyncNameProvider syncNameProvider)
        {
            try
            {
                if (!staxProperties.Any() || contentType == null)
                {
                    return null;
                }

                string? propertyId = syncNameProvider.IdPropertyName(contentType);

                return staxProperties.FirstOrDefault(x => x.Key == propertyId).Value.ToString();
            }
            catch (Exception)
            {
                //todo: Exception caused by Content Types not being in OC e.g. ESCO__MemberData.
                //To be rectified in a follow up story
                return null;
            }
        }
    }
}
