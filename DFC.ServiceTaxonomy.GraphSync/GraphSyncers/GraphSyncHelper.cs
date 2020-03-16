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

        public async Task<IEnumerable<string>> NodeLabels()
        {
            CheckPreconditions();

            string nodeLabel = await TransformOrDefault(GraphSyncPartSettings!.NodeNameTransform, _contentType!);

            return new[] {nodeLabel, CommonNodeLabel};
        }

        public async Task<IEnumerable<string>> NodeLabels(string contentType)
        {
            var graphSyncPartSettings = GetGraphSyncPartSettings(contentType);

            string nodeLabel = await TransformOrDefault(graphSyncPartSettings.NodeNameTransform, contentType);

            return new[] {nodeLabel, CommonNodeLabel};
        }

        // should only be used for fallbacks
        //todo: should we support fallback, or insist on relationship type being specified. don't need to set value as is in recipe
        public async Task<string> RelationshipType(string destinationContentType)
        {
            CheckPreconditions();

            //todo: check how this is called and if graphsync settings is valid

            var graphSyncPartSettings = GetGraphSyncPartSettings(destinationContentType);

            return string.IsNullOrEmpty(destinationContentType)
                ? $"has{destinationContentType}"
                : await Transform(graphSyncPartSettings.CreateRelationshipType!, destinationContentType);
        }

        public async Task<string> PropertyName(string name)
        {
            CheckPreconditions();

            return await TransformOrDefault(GraphSyncPartSettings!.PropertyNameTransform, name);
        }

        public string IdPropertyName
        {
            get
            {
                CheckPreconditions();

                return GraphSyncPartSettings!.IdPropertyName ?? "UserId";
            }
        }

        public async Task<string> IdPropertyValue(dynamic graphSyncContent)
        {
            CheckPreconditions();

            //todo: null Text.ToString()?
            //todo: pass selected prefix, or remove prefix and specify in transform?? <= latter

            return await TransformOrDefault(
                GraphSyncPartSettings!.IdPropertyValueTransform,
                graphSyncContent.Text.ToString());
        }

        private void CheckPreconditions()
        {
            if (_contentType == null)
                throw new InvalidOperationException($"You must set {nameof(ContentType)} before calling.");
        }

        private async Task<string> TransformOrDefault(string? transformCode, string value)
        {
            return string.IsNullOrEmpty(transformCode)
               || transformCode == "Value"
               || transformCode == "$\"{Value}\""
                ? value
                : await Transform(transformCode, value);
        }

        private async Task<string> Transform(string transformCode, string untransformedValue)
        {
            _graphSyncHelperCSharpScriptGlobals.Value = untransformedValue;
            return await CSharpScript.EvaluateAsync<string>(transformCode, globals: _graphSyncHelperCSharpScriptGlobals);
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
