﻿using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphVisualiser.Models;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Configuration;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Services
{
    public class OrchardToOwlGeneratorService : OwlDataGeneratorService, IOrchardToOwlGeneratorService
    {
        private const string NcsJobProfile = "JobProfile";

        private IEnumerable<ContentTypeDefinition> contentTypeDefinitions = Enumerable.Empty<ContentTypeDefinition>();

        public OrchardToOwlGeneratorService(IOptionsMonitor<OwlDataGeneratorConfigModel> owlDataGeneratorConfigModel) : base(owlDataGeneratorConfigModel) { }

        public OwlDataModel? CreateOwlDataModels(IEnumerable<ContentTypeDefinition> contentTypeDefinitions)
        {
            this.contentTypeDefinitions = contentTypeDefinitions;

            var selectedContentTypeDefinition = GetContentTypeDefinition(NcsJobProfile);

            if (selectedContentTypeDefinition != null)
            {
                TransformData(selectedContentTypeDefinition);

                var result = new OwlDataModel
                {
                    Namespace = CreateNamespaces(),
                    Header = CreateHeader(),
                    Settings = CreateSettings(),
                    Class = nodeDataModels.Select(n => CreateClass(n, selectedContentTypeDefinition.Name)).ToList(),
                    ClassAttribute = nodeDataModels.Select(CreateClassAttribute).ToList(),
                    Property = relationshipDataModels.Select(CreateProperty).ToList(),
                    PropertyAttribute = relationshipDataModels.Select(CreatePropertyAttribute).ToList(),
                };

                return result;
            }

            return null;
        }

        private void TransformData(ContentTypeDefinition contentTypeDefinition)
        {
            var nodeModels = new List<NodeDataModel>();
            var relationshipModels = new List<RelationshipDataModel>();

            nodeModels.Add(TransformContentTypeDefinitionToNode(contentTypeDefinition, nodeModels.Count + 1, $"Class{contentTypeDefinition.Name}"));

            foreach (var contentTypePartItem in contentTypeDefinition.Parts)
            {
                var bagPartSettings = contentTypePartItem.GetSettings<BagPartSettings>();
                var fields = contentTypePartItem.PartDefinition.Fields;
                var linkedName = bagPartSettings.ContainedContentTypes.Length == 0 ? $"Part{contentTypePartItem.PartDefinition.Name}" : $"Bag{contentTypePartItem.Name}";
                var id = bagPartSettings.ContainedContentTypes.Length == 0 ? $"Part{contentTypePartItem.PartDefinition.Name}" : $"Bag{contentTypePartItem.Name}";

                nodeModels.Add(TransformContentTypePartDefinitionToNode(contentTypePartItem, nodeModels.Count + 1, id));

                relationshipModels.Add(TransformContentTypePartDefinitionToRelationship(contentTypePartItem, $"Class{contentTypeDefinition.Name}", linkedName));

                foreach (var name in bagPartSettings.ContainedContentTypes)
                {
                    var bagContentTypeDefinition = GetContentTypeDefinition(name);

                    if (bagContentTypeDefinition != null)
                    {
                        var partId = $"Part{bagContentTypeDefinition.Name}";

                        nodeModels.Add(TransformContentTypeDefinitionToNode(bagContentTypeDefinition, nodeModels.Count + 1, partId));

                        relationshipModels.Add(TransformContentTypeDefinitionToRelationship(bagContentTypeDefinition, linkedName, partId));
                    }
                }

                if (fields.Any())
                {
                    foreach (var field in fields)
                    {
                        var fieldId = $"Field{field.Name}";

                        nodeModels.Add(TransformContentPartFieldDefinitionToNode(field, nodeModels.Count + 1, fieldId));

                        relationshipModels.Add(TransformContentPartFieldDefinitionToRelationship(field, linkedName, fieldId));
                    }
                }
            }

            nodeDataModels = nodeModels;
            relationshipDataModels = relationshipModels;
        }

        private NodeDataModel TransformContentTypeDefinitionToNode(ContentTypeDefinition contentTypeDefinition, long key, string id)
        {
            return new NodeDataModel(
                id, key, contentTypeDefinition.Name, contentTypeDefinition.DisplayName, contentTypeDefinition.DisplayName);
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
            return new NodeDataModel(
                id, key, contentPartItem.PartDefinition.Name,
                contentPartItem.PartDefinition.GetSettings<ContentPartSettings>()?.DisplayName ?? contentPartItem.Name,
                contentPartItem.PartDefinition.GetSettings<ContentPartSettings>()?.Description ?? contentPartItem.Name);
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

        private NodeDataModel TransformContentPartFieldDefinitionToNode(ContentPartFieldDefinition contentPartFieldDefinition, long key, string id)
        {
            ContentPartFieldSettings? contentPartFieldSettings = contentPartFieldDefinition.GetSettings<ContentPartFieldSettings>();

            return new NodeDataModel(id, key, contentPartFieldDefinition.Name,
                contentPartFieldSettings?.DisplayName ?? contentPartFieldDefinition.Name,
                contentPartFieldSettings?.Description ?? contentPartFieldDefinition.Name);
        }

        private RelationshipDataModel TransformContentPartFieldDefinitionToRelationship(ContentPartFieldDefinition contentPartFieldDefinition, string linkFromName, string linkToName)
        {
            return new RelationshipDataModel
            {
                Id = contentPartFieldDefinition.Name,
                Label = contentPartFieldDefinition.GetSettings<ContentPartFieldSettings>()?.DisplayName ?? contentPartFieldDefinition.Name,
                Domain = linkFromName,
                Range = linkToName,
            };
        }

        private ContentTypeDefinition? GetContentTypeDefinition(string name)
        {
            return contentTypeDefinitions?.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
