using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Generators;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

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
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartGraphSyncer> _partSyncers;
        private readonly IGraphSyncPartIdProperty _graphSyncPartIdProperty;

        public GraphSyncer(
            IGraphDatabase graphDatabase,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPartGraphSyncer> partSyncers,
            IGraphSyncPartIdProperty graphSyncPartIdProperty)
        {
            _graphDatabase = graphDatabase;
            _contentDefinitionManager = contentDefinitionManager;
            _partSyncers = partSyncers;
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
        }

        //todo: have as setting of activity, or graph sync content part settings
        private const string NcsPrefix = "ncs__";

        public async Task SyncToGraph(ContentItem contentItem)
        {
            // we use the existence of a GraphSync content part as a marker to indicate that the content item should be synced
            // so we silently noop if it's not present
            dynamic graphSyncPartContent = ((JObject) contentItem.Content)[nameof(GraphSyncPart)];
            //todo: text -> id?
            //todo: why graph sync has tags in features, others don't?
            if (graphSyncPartContent == null)
                return;

            //addtransient & private class instance?
            var nodeProperties = new Dictionary<string, object>();
            var relationships = new Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>>();

            await AddContentPartSyncComponents(contentItem, nodeProperties, relationships);

            await SyncComponentsToGraph(contentItem, nodeProperties, relationships, _graphSyncPartIdProperty.Name, _graphSyncPartIdProperty.Value(graphSyncPartContent));
        }

        private async Task AddContentPartSyncComponents(
            ContentItem contentItem,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> relationships)
        {
            foreach (var partSync in _partSyncers)
            {
                string partName = partSync.PartName ?? contentItem.ContentType;

                dynamic partContent = contentItem.Content[partName];
                if (partContent == null)
                    continue;

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
                var contentTypePartDefinition =
                    contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == partName);

                await partSync.AddSyncComponents(partContent, nodeProperties, relationships, contentTypePartDefinition);
            }
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
            Query mergeNodesQuery = QueryGenerator.MergeNode(nodeLabel, nodeProperties, sourceIdPropertyName);
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
    }
}
