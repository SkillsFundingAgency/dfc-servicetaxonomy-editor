using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    // we group methods by whether they work off the set ContentType property, or pass in a contentType
#pragma warning disable S4136

    //todo: better name
    public class GraphSyncHelper : IGraphSyncHelper
    {
        //todo: gotta be careful about lifetimes. might have to injectiserviceprovider
        private readonly IGraphSyncHelperCSharpScriptGlobals _graphSyncHelperCSharpScriptGlobals;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private string? _contentType;
        //todo: make this public if everything that needs it doesn't get moved into here?
        private GraphSyncPartSettings? _graphSyncPartSettings;

        //todo: from config CommonNodeLabels
        private const string CommonNodeLabel = "Resource";

        public GraphSyncHelper(
            IGraphSyncHelperCSharpScriptGlobals graphSyncHelperCSharpScriptGlobals,
            IContentDefinitionManager contentDefinitionManager)
        {
            _graphSyncHelperCSharpScriptGlobals = graphSyncHelperCSharpScriptGlobals;
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

        public GraphSyncPartSettings GraphSyncPartSettings
        {
            get
            {
                CheckPreconditions();

                return _graphSyncPartSettings!;
            }
        }


        // property instead?
        public async Task<IEnumerable<string>> NodeLabels()
        {
            CheckPreconditions();

            //todo:
            string nodeLabel = string.IsNullOrEmpty(GraphSyncPartSettings!.NodeNameTransform)
                               || GraphSyncPartSettings!.NodeNameTransform == "Value"
                ? _contentType!
                : await Transform(GraphSyncPartSettings!.NodeNameTransform, _contentType!);

            return new[] {nodeLabel, CommonNodeLabel};
        }

        private async Task<string> Transform(string transformCode, string untransformedValue)
        {
            _graphSyncHelperCSharpScriptGlobals.Value = untransformedValue;
            return await CSharpScript.EvaluateAsync<string>(transformCode, globals: _graphSyncHelperCSharpScriptGlobals);
        }

        public IEnumerable<string> NodeLabels(string contentType)
        {
            //todo: Transform
            string NcsPrefix = "ncs__";
            return new[] {NcsPrefix + contentType, CommonNodeLabel};
        }

        public string RelationshipType(string destinationContentType)
        {
            //todo: add transform for default relationshipname to settings
            string NcsPrefix = "ncs__";
            return $"{NcsPrefix}has{destinationContentType}";
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

                return GraphSyncPartSettings!.IdPropertyName ?? "UserId";
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

#pragma warning restore S4136
}
