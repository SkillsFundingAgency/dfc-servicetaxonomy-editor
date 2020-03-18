using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphVisualiser.Models;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Services
{
    public class OrchardToOwlGeneratorService : OwlDataGeneratorService, IOrchardToOwlGeneratorService
    {
        private const string NcsJobProfile = "JobProfile";

        private IEnumerable<ContentTypeDefinition> contentTypeDefinitions;

        public OwlDataModel CreateOwlDataModels(IEnumerable<ContentTypeDefinition> contentTypeDefinitions)
        {
            this.contentTypeDefinitions = contentTypeDefinitions;

            var selectedContentTypeDefinition = GetContentTypeDefinition(NcsJobProfile);

            TransformData(selectedContentTypeDefinition);

            var result = new OwlDataModel
            {
                Namespace = CreateNamespaces(),
                Header = CreateHeader(),
                Settings = CreateSettings(),
                Class = nodeDataModels.Select(n => CreateClass(n, selectedContentTypeDefinition.Name)).ToList(),
                ClassAttribute = nodeDataModels.Select(n => CreateClassAttribute(n)).ToList(),
                Property = relationshipDataModels.Select(r => CreateProperty(r)).ToList(),
                PropertyAttribute = relationshipDataModels.Select(r => CreatePropertyAttribute(r)).ToList(),
            };

            return result;
        }

        private void TransformData(ContentTypeDefinition contentTypeDefinition)
        {
            var nodeModels = new List<NodeDataModel>();
            var relationshipModels = new List<RelationshipDataModel>();

            nodeModels.Add(TransformContentTypeDefinitionToNode(contentTypeDefinition, nodeModels.Count + 1, $"Class{contentTypeDefinition.Name}"));

            foreach (var contentTypePartItem in contentTypeDefinition.Parts)
            {
                var bagPartSettings = contentTypePartItem.GetSettings<BagPartSettings>();
                var linkedName = bagPartSettings.ContainedContentTypes.Length == 0 ? $"Part{contentTypePartItem.PartDefinition.Name}" : $"Bag{contentTypePartItem.Name}";
                var id = bagPartSettings.ContainedContentTypes.Length == 0 ? $"Part{contentTypePartItem.PartDefinition.Name}" : $"Bag{contentTypePartItem.Name}";

                nodeModels.Add(TransformContentTypePartDefinitionToNode(contentTypePartItem, nodeModels.Count + 1, id));

                relationshipModels.Add(TransformContentTypePartDefinitionToRelationship(contentTypePartItem, $"Class{contentTypeDefinition.Name}", linkedName));

                foreach (var name in bagPartSettings.ContainedContentTypes)
                {
                    var bagContentTypeDefinition = GetContentTypeDefinition(name);
                    var partId = $"Part{bagContentTypeDefinition.Name}";

                    nodeModels.Add(TransformContentTypeDefinitionToNode(bagContentTypeDefinition, nodeModels.Count + 1, partId));

                    relationshipModels.Add(TransformContentTypeDefinitionToRelationship(bagContentTypeDefinition, linkedName, $"Part{bagContentTypeDefinition.Name}"));
                }
            }

            nodeDataModels = nodeModels;
            relationshipDataModels = relationshipModels;
        }

        private NodeDataModel TransformContentTypeDefinitionToNode(ContentTypeDefinition contentTypeDefinition, long key, string id)
        {
            var result = new NodeDataModel
            {
                Id = id,
                Key = key,
                Type = contentTypeDefinition.Name,
                Label = contentTypeDefinition.DisplayName,
                Comment = contentTypeDefinition.DisplayName,
            };

            return result;
        }

        private RelationshipDataModel TransformContentTypePartDefinitionToRelationship(ContentTypePartDefinition contentTypePartDefinition, string linkFromName, string linkToName)
        {
            var relationshipDataModel = new RelationshipDataModel
            {
                Id = linkToName,
                Label = contentTypePartDefinition.PartDefinition.GetSettings<ContentPartSettings>()?.Description ?? contentTypePartDefinition.Name,
                Domain = linkFromName,
                Range = linkToName,
            };

            return relationshipDataModel;
        }

        private NodeDataModel TransformContentTypePartDefinitionToNode(ContentTypePartDefinition contentPartItem, long key, string id)
        {
            string type = contentPartItem.PartDefinition.Name;
            var result = new NodeDataModel
            {
                Id = id,
                Key = key,
                Type = type,
                Label = contentPartItem.PartDefinition.GetSettings<ContentPartSettings>()?.DisplayName ?? contentPartItem.Name,
                Comment = contentPartItem.PartDefinition.GetSettings<ContentPartSettings>()?.Description ?? contentPartItem.Name,
            };

            return result;
        }

        private RelationshipDataModel TransformContentTypeDefinitionToRelationship(ContentTypeDefinition contentTypeDefinition, string linkFromName, string linkToName)
        {
            var relationshipDataModel = new RelationshipDataModel
            {
                Id = contentTypeDefinition.Name,
                Label = contentTypeDefinition.DisplayName,
                Domain = linkFromName,
                Range = linkToName,
            };

            return relationshipDataModel;
        }

        private ContentTypeDefinition GetContentTypeDefinition(string name)
        {
            var result = contentTypeDefinitions?.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            return result;
        }
    }
}
