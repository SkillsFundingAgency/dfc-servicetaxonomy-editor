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
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    //todo: don't need ContentApiPrefix setting in sites/default anymore

    // we group methods by whether they work off the set ContentType property, or pass in a contentType
    //todo: better to break out into separate classes??
#pragma warning disable S4136

    public class GraphSyncHelper : IGraphSyncHelper
    {
        //todo: gotta be careful about lifetimes. might have to inject iserviceprovider
        private readonly IGraphSyncHelperCSharpScriptGlobals _graphSyncHelperCSharpScriptGlobals;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private string? _contentType;
        private GraphSyncPartSettings? _graphSyncPartSettings;
        private readonly Stack<Func<string, string>> _propertyNameTransformers;

        //todo: from config CommonNodeLabels
        private const string CommonNodeLabel = "Resource";

        public GraphSyncHelper(
            IGraphSyncHelperCSharpScriptGlobals graphSyncHelperCSharpScriptGlobals,
            IContentDefinitionManager contentDefinitionManager)
        {
            _graphSyncHelperCSharpScriptGlobals = graphSyncHelperCSharpScriptGlobals;
            _contentDefinitionManager = contentDefinitionManager;
            _propertyNameTransformers = new Stack<Func<string, string>>();
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

        public DisposeAction PushPropertyNameTransform(Func<string, string> nodePropertyTranformer)
        {
            _propertyNameTransformers.Push(nodePropertyTranformer);
            return new DisposeAction(() => PopPropertyNameTransform());
        }

        public void PopPropertyNameTransform()
        {
            _propertyNameTransformers.Pop();
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
            //todo: rename NodeNameTransform to NodeLabelTransform
            string nodeLabel = graphSyncPartSettings.NodeNameTransform switch
            {
                "ContentType" => contentType,
                "$\"esco__{ContentType}\"" => $"esco__{contentType}",
                _ => await TransformOrDefault(graphSyncPartSettings.NodeNameTransform, contentType, contentType)
            };

            return new[] { nodeLabel, CommonNodeLabel };
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

            string propertyName = GraphSyncPartSettings!.PropertyNameTransform switch
            {
                "Value" => name,
                _ => await TransformOrDefault(GraphSyncPartSettings!.PropertyNameTransform, name, _contentType!)
            };

            //todo: check order. want first added to come last
            //todo: need to do the same for RelationshipTypeDefault too

            return _propertyNameTransformers.Aggregate(propertyName,
                (current, transformer) => transformer(current));
        }

        //todo: rename to NodeIdPropertyName
        public string IdPropertyName()
        {
            CheckPreconditions();

            return IdPropertyName(GraphSyncPartSettings!);
        }

        public string IdPropertyName(string contentType)
        {
            return IdPropertyName(GetGraphSyncPartSettings(contentType));
        }

        private string IdPropertyName(GraphSyncPartSettings graphSyncPartSettings)
        {
            return graphSyncPartSettings.IdPropertyName ?? "userId";
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

                "$\"<<ContentApiPrefix>>/{ContentType}/{Value}\".ToLowerInvariant()" =>
                $"<<ContentApiPrefix>>/{_contentType}/{newGuid}".ToLowerInvariant(),

                _ => await TransformOrDefault(GraphSyncPartSettings!.GenerateIdPropertyValue, newGuid, _contentType!)
            };
        }

        public string ContentIdPropertyName => "Text";

        public object? GetIdPropertyValue(JObject graphSyncContent, IContentItemVersion contentItemVersion)
        {
            object? untransformedIdValue = graphSyncContent[ContentIdPropertyName]?.ToObject<object?>();

            string? untransformedIdString = untransformedIdValue as string;
            if (untransformedIdString == null)
                return untransformedIdValue;

            return untransformedIdString.Replace("<<ContentApiPrefix>>", contentItemVersion.ContentApiBaseUrl);
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
