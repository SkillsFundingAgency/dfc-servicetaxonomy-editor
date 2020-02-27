using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using YesSql;

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
        private readonly IMergeNodeCommand _mergeNodeCommand;
        private readonly IReplaceRelationshipsCommand _replaceRelationshipsCommand;
        private readonly IDeleteNodeCommand _deleteNodeCommand;
        private readonly ISession _session;

        public GraphSyncer(
            IGraphDatabase graphDatabase,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPartGraphSyncer> partSyncers,
            IGraphSyncPartIdProperty graphSyncPartIdProperty,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IDeleteNodeCommand deleteNodeCommand,
            ISession session)
        {
            _graphDatabase = graphDatabase;
            _contentDefinitionManager = contentDefinitionManager;
            _partSyncers = partSyncers;
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
            _mergeNodeCommand = mergeNodeCommand;
            _replaceRelationshipsCommand = replaceRelationshipsCommand;
            _deleteNodeCommand = deleteNodeCommand;
            _session = session;
        }

        //todo: have as setting of activity, or graph sync content part settings
        private const string NcsPrefix = "ncs__";
        private const string CommonNodeLabel = "Resource";

        public async Task SyncToGraph(ContentItem contentItem)
        {
            // we use the existence of a GraphSync content part as a marker to indicate that the content item should be synced
            // so we silently noop if it's not present
            dynamic? graphSyncPartContent = ((JObject) contentItem.Content)[nameof(GraphSyncPart)];
            //todo: text -> id?
            //todo: why graph sync has tags in features, others don't?
            if (graphSyncPartContent == null)
                return;

            // could inject _graphSyncPartIdProperty into mergeNodeCommand, but should we?

            _mergeNodeCommand.NodeLabels.Add(NcsPrefix + contentItem.ContentType);
            _mergeNodeCommand.NodeLabels.Add(CommonNodeLabel);
            _mergeNodeCommand.IdPropertyName = _graphSyncPartIdProperty.Name;

            await AddContentPartSyncComponents(contentItem);

            await SyncComponentsToGraph(graphSyncPartContent);
        }

        public async Task DeleteFromGraph(ContentItem contentItem)
        {
            _deleteNodeCommand.ContentType = contentItem.ContentType;
            _deleteNodeCommand.Uri = contentItem.Content.GraphSyncPart.Text;

            try
            {
                //TODO : check if there are any incoming relationships to the node being deleted
                await _graphDatabase.RunWriteQueries(_deleteNodeCommand.Query);
            }
            //TODO : check what exceptions are thrown from the GraphDB abstraction
            catch
            {
                //this forces a rollback of the currect OC operation db transaction.
                //however, it doesn't notify the UI that the operation failed, so it still displays a success message
                //TODO : find out how to hide the success message!!!
                _session.Cancel();

                throw;
            }
        }

        private async Task AddContentPartSyncComponents(ContentItem contentItem)
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

                await partSync.AddSyncComponents(partContent, _mergeNodeCommand.Properties, _replaceRelationshipsCommand.Relationships, contentTypePartDefinition);
            }
        }

        private async Task SyncComponentsToGraph(dynamic graphSyncPartContent)
        {
            List<Query> queries = new List<Query> {_mergeNodeCommand.Query};

            if (_replaceRelationshipsCommand.Relationships.Any())
            {
                // doesn't really belong here...
                _replaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(_mergeNodeCommand.NodeLabels);
                _replaceRelationshipsCommand.SourceIdPropertyName = _mergeNodeCommand.IdPropertyName;
                _replaceRelationshipsCommand.SourceIdPropertyValue = _graphSyncPartIdProperty.Value(graphSyncPartContent);

                queries.Add(_replaceRelationshipsCommand.Query);
            }

            await _graphDatabase.RunWriteQueries(queries.ToArray());
        }
    }
}
