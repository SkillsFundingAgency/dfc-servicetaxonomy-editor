using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Flows.Models;

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

        public GraphSyncer(
            IGraphDatabase graphDatabase,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPartGraphSyncer> partSyncers,
            IGraphSyncPartIdProperty graphSyncPartIdProperty,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand)
        {
            _graphDatabase = graphDatabase;
            _contentDefinitionManager = contentDefinitionManager;
            _partSyncers = partSyncers;
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
            _mergeNodeCommand = mergeNodeCommand;
            _replaceRelationshipsCommand = replaceRelationshipsCommand;
        }

        //todo: have as setting of activity, or graph sync content part settings
        private const string NcsPrefix = "ncs__";
        private const string CommonNodeLabel = "Resource";

        public async Task<IMergeNodeCommand?> SyncToGraph(string contentType, JObject content)
        {
            // we use the existence of a GraphSync content part as a marker to indicate that the content item should be synced
            // so we silently noop if it's not present
            dynamic? graphSyncPartContent = content[nameof(GraphSyncPart)];
            //todo: text -> id?
            //todo: why graph sync has tags in features, others don't?
            if (graphSyncPartContent == null)
                return null;

            // could inject _graphSyncPartIdProperty into mergeNodeCommand, but should we?

            _mergeNodeCommand.NodeLabels.Add(NcsPrefix + contentType);
            _mergeNodeCommand.NodeLabels.Add(CommonNodeLabel);
            _mergeNodeCommand.IdPropertyName = _graphSyncPartIdProperty.Name;

            await AddContentPartSyncComponents(contentType, content);

            await SyncComponentsToGraph(graphSyncPartContent);

            return _mergeNodeCommand;
        }

        private async Task AddContentPartSyncComponents(string contentType, JObject content)
        {
            // ensure graph sync part is processed first, as other part syncers (current bagpart) require the node's id value
            string graphSyncPartName = nameof(GraphSyncPart);
            var partSyncersWithGraphLookupFirst
                = _partSyncers.Where(ps => ps.PartName != graphSyncPartName)
                    .Prepend(_partSyncers.First(ps => ps.PartName == graphSyncPartName));

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);

            foreach (var partSync in partSyncersWithGraphLookupFirst)
            {
                string partName = partSync.PartName ?? contentType;

                dynamic? partContent = content[partName];
                if (partContent == null)
                {
                    // we've either hit a part's content item and there's no associated part syncer
                    // or we've hit a named bag part
                    //todo: find a better way to identify a named bag part : this will only work if there is a max of 1 named bag part in the content type
                    if (partName != nameof(BagPart))
                        continue;

                    //todo: don't need to check already processed content
                    foreach (JToken partContentJToken in content.Values())
                    {
                        // when we've recursed into a bag, each value is a propert, rather than a part
                        if (partContentJToken.HasValues && partContentJToken["ContentItems"] != null)
                        {
                            partContent = partContentJToken;
                            partName = partContentJToken.Path;
                            break;
                        }
                    }
                    // either we're checking for a bagpart and didn't find a likely candidate
                    // or there's no bag part in this contenttype
                    // we have to skip this part, in case there's no bag part in the type
                    // but it does mean we silently ignore (not sync) a bagpart if we can't find approp. data
                    //todo: get and check the contenttype definition
                    if (partContent == null)
                        continue;
                        //throw new GraphSyncException("Couldn't find content for named BagPart.");
                }

                // bag part has p.Name == <<name>>, p.PartDefinition.Name == "BagPart"
                // (other non-named parts have the part name in both)
                // we look for the name, so at least for this line, we support >1 named bag parts :-)
                var contentTypePartDefinition =
                    contentTypeDefinition.Parts.FirstOrDefault(p => p.Name == partName);

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
