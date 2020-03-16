using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    //todo: better name
    public class GraphSyncHelper : IGraphSyncHelper
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        //todo: make this public if everything that needs it doesn't get moved into here?
        private GraphSyncPartSettings? _graphSyncPartSettings;
        private string? _contentType;

        //todo: from config CommonNodeLabels
        private const string CommonNodeLabel = "Resource";

        public GraphSyncHelper(
            IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public string? ContentType
        {
            get
            {
                return _contentType;
            }
            set
            {
                _contentType = value ?? throw new ArgumentNullException($"Null is not an acceptable value for {nameof(ContentType)}.");
                _graphSyncPartSettings = GetGraphSyncPartSettings(value);
            }
        }

        // property instead?
        public IEnumerable<string> NodeLabels
        {
            get
            {
                CheckPreconditions();

                string nodeLabel = string.IsNullOrEmpty(_graphSyncPartSettings!.NodeNameTransform)
                                   || _graphSyncPartSettings!.NodeNameTransform == "val"
                    ? _contentType!
                    : "todo";

                return new[] {nodeLabel, CommonNodeLabel};
            }
        }

        public string PropertyName(string name)
        {
            //todo: PropertyNameTransform
            string NcsPrefix = "ncs__";

            return NcsPrefix + name;
        }

        public string IdPropertyName
        {
            get
            {
                CheckPreconditions();

                return _graphSyncPartSettings!.IdPropertyName ?? "UserId";
            }
        }

        public string? IdPropertyValue(dynamic graphSyncContent)
        {
            //todo: IdPropertyValueTransform
            return graphSyncContent.Text.ToString();
        }

        private void CheckPreconditions()
        {
            if (_contentType == null)
                throw new InvalidOperationException($"You must set {nameof(ContentType)} before calling.");
        }

        private GraphSyncPartSettings GetGraphSyncPartSettings(string contentType)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphSyncPart));
            return contentTypePartDefinition.GetSettings<GraphSyncPartSettings>();
        }
    }
}
