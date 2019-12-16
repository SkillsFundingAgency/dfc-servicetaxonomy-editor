using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Generators;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    // Type mappings
    // -------------
    // OC UI Field Type | OC Content | Neo Driver    | Cypher     | NSMNTX postfix | RDF             | Notes
    // Boolean            ?            see notes       Boolean                       xsd:boolean       neo docs say driver is boolean. do they mean Boolean or bool?
    // Content Picker
    // Date
    // Date Time
    // Html               Html         string          String
    // Link
    // Markdown
    // Media
    // Numeric            Value        long            Integer                       xsd:integer       \ OC UI has only numeric, which it presents as a real in content. do we always consistently map to a long or float, or allow user to differentiate with metadata?
    // Numeric            Value        float           Float                                           / (RDF supports xsd:int & xsd:integer, are they different or synonyms)
    // Text               Text         string          String                        xsd:string        no need to specify in RDF - default?
    // Time
    // Youtube
    //
    // see
    // https://github.com/neo4j/neo4j-dotnet-driver
    // https://www.w3.org/2011/rdf-wg/wiki/XSD_Datatypes
    // https://neo4j.com/docs/labs/nsmntx/current/import/

    public class GraphSyncer : IGraphSyncer
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartGraphSyncer> _partSyncers;
        private readonly Regex _relationshipTypeRegex;

        public GraphSyncer(
            IGraphDatabase graphDatabase,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPartGraphSyncer> partSyncers)
        {
            _graphDatabase = graphDatabase;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _partSyncers = partSyncers;

            _relationshipTypeRegex = new Regex("\\[:(.*?)\\]", RegexOptions.Compiled);
        }

        //todo: have as setting of activity, or graph sync content part settings
        private const string NcsPrefix = "ncs__";

        public async Task SyncToGraph(ContentItem contentItem)
        {
            // we use the existence of a GraphSync content part as a marker to indicate that the content item should be synced
            // so we silently noop if it's not present
            dynamic graphSyncPartContent = ((JObject) contentItem.Content)["GraphSyncPart"];
            //todo: text -> id?
            //todo: why graph sync has tags in features, others don't?
            if (graphSyncPartContent == null)
                return;

            //addtransient & private class instance?
            var nodeProperties = new Dictionary<string, object>();
            var relationships = new Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>>();

            AddContentPartSyncComponents(contentItem, nodeProperties, relationships);

            await AddContentFieldSyncComponents(contentItem, nodeProperties, relationships);

            // or inject directly -> will work differently once syncing is delegated
            // delegate (partial) decision to GraphSyncPartGraphSyncer?
            var graphSyncPartGraphSyncer = (GraphSyncPartGraphSyncer)_partSyncers.First(s => s.PartName == "GraphSyncPart");
            await SyncComponentsToGraph(contentItem, nodeProperties, relationships, graphSyncPartGraphSyncer.IdPropertyName, graphSyncPartGraphSyncer.IdPropertyValue(graphSyncPartContent));
        }

        private async Task SyncComponentsToGraph(
            ContentItem contentItem,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType),IEnumerable<string>> relationships,
            string sourceIdPropertyName,
            string sourceIdPropertyValue)
        {
            string nodeLabel = NcsPrefix + contentItem.ContentType;

            // could create ienumerable and have 1 call
            Query mergeNodesQuery = QueryGenerator.MergeNodes(nodeLabel, nodeProperties);
            if (relationships.Any())
            {
                await _graphDatabase.RunWriteQueries(mergeNodesQuery,
                    QueryGenerator.MergeRelationships(nodeLabel, sourceIdPropertyName, sourceIdPropertyValue, relationships));
            }
            else
            {
                await _graphDatabase.RunWriteQueries(mergeNodesQuery);
            }
        }

        private void AddContentPartSyncComponents(
            ContentItem contentItem,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> relationships)
        {
            foreach (var partSync in _partSyncers)
            {
                dynamic partContent = contentItem.Content[partSync.PartName];
                if (partContent == null)
                    continue;

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
                var contentTypePartDefinition =
                    contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == partSync.PartName);

                partSync.AddSyncComponents(partContent, nodeProperties, relationships, contentTypePartDefinition);
            }
        }

        //todo: content fields are contained in a part, could move into part syncer (with well known name?)
        private async Task AddContentFieldSyncComponents(
            ContentItem contentItem,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> relationships)
        {
            foreach (dynamic? field in contentItem.Content[contentItem.ContentType])
            {
                if (field == null)
                    continue;

                var fieldTypeAndValue = (JProperty?) ((JProperty) field).First?.First;
                if (fieldTypeAndValue == null)
                    continue;

                switch (fieldTypeAndValue.Name)
                {
                    // we map from Orchard Core's types to Neo4j's driver types (which map to cypher type)
                    // see remarks to view mapping table
                    // we might also want to map to rdf types here (accept flag to say store with type?)
                    // will be useful if we import into neo using keepCustomDataTypes
                    // we can append the datatype to the value, i.e. value^^datatype
                    // see https://neo4j-labs.github.io/neosemantics/#_handling_custom_data_types

                    case "Text":
                    case "Html":
                        nodeProperties.Add(NcsPrefix + field.Name, fieldTypeAndValue.Value.ToString());
                        break;
                    case "Value":
                        // orchard always converts entered value to real 2.0 (float/double/decimal)
                        // todo: how to decide whether to convert to driver/cypher's long/integer or float/float? metadata field to override default of int to real?

                        nodeProperties.Add(NcsPrefix + field.Name, (long) fieldTypeAndValue.Value.ToObject(typeof(long)));
                        break;
                    case "ContentItemIds":
                        await AddContentPickerFieldSyncComponents(contentItem, relationships, field, fieldTypeAndValue);
                        break;
                }
            }
        }

        private async Task AddContentPickerFieldSyncComponents(
            ContentItem contentItem,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> relationships,
            dynamic field,
            JProperty fieldTypeAndValue)
        {
            //todo: check for empty list => noop, except for initial delete

            string? relationshipType = null;
            ContentTypeDefinition contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            IEnumerable<ContentPartFieldDefinition> contentPartDefinitions
                = contentTypeDefinition.Parts.First(p => p.Name == contentItem.ContentType).PartDefinition.Fields;
            ContentPartFieldDefinition contentPartDefinition = contentPartDefinitions.First(d => d.Name == field.Name);
            string? contentPartHint = contentPartDefinition.Settings["ContentPickerFieldSettings"]?["Hint"]?.ToString();
            if (contentPartHint != null)
            {
                Match match = _relationshipTypeRegex.Match(contentPartHint);
                if (match.Success)
                {
                    relationshipType = $"{match.Groups[1].Value}";
                }
            }

            string? destNodeLabel = null;
            var destUris = new List<string>();
            foreach (JToken relatedContentId in fieldTypeAndValue.Value)
            {
                ContentItem relatedContent =
                    await _contentManager.GetAsync(relatedContentId.ToString(), VersionOptions.Latest);
                string relatedContentKey = relatedContent.Content.UriId.URI.Text.ToString();
                destUris.Add(relatedContentKey.ToString());

                //todo: don't repeat
                destNodeLabel = NcsPrefix + relatedContent.ContentType;
                if (relationshipType == null)
                    relationshipType = $"{NcsPrefix}has{relatedContent.ContentType}";
            }

            if (destNodeLabel != null && relationshipType != null)
                relationships.Add((destNodeLabel, "uri", relationshipType), destUris);
        }
    }
}
