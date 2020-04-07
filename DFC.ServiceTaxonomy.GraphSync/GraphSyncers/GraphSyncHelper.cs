using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    // we group methods by whether they work off the set ContentType property, or pass in a contentType
#pragma warning disable S4136

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

            return await NodeLabels(GraphSyncPartSettings!, _contentType!);
        }

        public async Task<IEnumerable<string>> NodeLabels(string contentType)
        {
            var graphSyncPartSettings = GetGraphSyncPartSettings(contentType);

            return await NodeLabels(graphSyncPartSettings, contentType);
        }

        private async Task<IEnumerable<string>> NodeLabels(GraphSyncPartSettings graphSyncPartSettings, string contentType)
        {
            string nodeLabel = graphSyncPartSettings.NodeNameTransform switch
            {
                "$\"ncs__{ContentType}\"" => $"ncs__{contentType}",
                "$\"esco__{ContentType}\"" => $"esco__{contentType}",
                _ => await TransformOrDefault(graphSyncPartSettings.NodeNameTransform, contentType, contentType)
            };

            return new[] {nodeLabel, CommonNodeLabel};
        }

        // should only be used for fallbacks
        public async Task<string> RelationshipTypeDefault(string destinationContentType)
        {
            CheckPreconditions();

            var graphSyncPartSettings = GetGraphSyncPartSettings(destinationContentType);

            return await TransformOrDefault(
                graphSyncPartSettings.CreateRelationshipType,
                $"has{destinationContentType}",
                destinationContentType);
        }

        public async Task<string> PropertyName(string name)
        {
            CheckPreconditions();

            return GraphSyncPartSettings!.PropertyNameTransform switch
            {
                "$\"ncs__{Value}\"" => $"ncs__{name}",
                _ => await TransformOrDefault(GraphSyncPartSettings!.PropertyNameTransform, name, _contentType!)
            };
        }

        public string IdPropertyName
        {
            get
            {
                CheckPreconditions();

                return GraphSyncPartSettings!.IdPropertyName ?? "UserId";
            }
        }

        //todo: rename other methods, Generate?
        public async Task<string> GenerateIdPropertyValue()
        {
            CheckPreconditions();

            string newGuid = Guid.NewGuid().ToString("D");

            return GraphSyncPartSettings!.GenerateIdPropertyValue switch
            {
                "$\"http://data.europa.eu/esco/{ContentType.ToLowerInvariant()}/{Value}\"" =>
                $"http://data.europa.eu/esco/{_contentType!.ToLowerInvariant()}/{newGuid}",

                "$\"http://nationalcareers.service.gov.uk/{ContentType.ToLowerInvariant()}/{Value}\"" =>
                $"http://nationalcareers.service.gov.uk/{_contentType!.ToLowerInvariant()}/{newGuid}",

                _ => await TransformOrDefault(GraphSyncPartSettings!.GenerateIdPropertyValue, newGuid, _contentType!)
            };
        }

        public object GetIdPropertyValue(dynamic graphSyncContent)
        {
            //todo: null Text.ToString()?

            //todo: need way to support id values of types other than text
            return graphSyncContent.Text.ToString();
        }

        private void CheckPreconditions()
        {
            if (_contentType == null)
                throw new InvalidOperationException($"You must set {nameof(ContentType)} before calling.");
        }

        private async Task<string> TransformOrDefault(string? transformCode, string value, string contentType)
        {
            return string.IsNullOrEmpty(transformCode)
               || transformCode == "Value"
               || transformCode == "$\"{Value}\""
                ? value
                : await Transform(transformCode, value, contentType);
        }

        private async Task<string> Transform(string transformCode, string untransformedValue, string contentType)
        {
            _graphSyncHelperCSharpScriptGlobals.Value = untransformedValue;
            _graphSyncHelperCSharpScriptGlobals.ContentType = contentType;

            return await CSharpScript.EvaluateAsync<string>(transformCode,
                ScriptOptions.Default.WithImports("System"),
                _graphSyncHelperCSharpScriptGlobals);
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
