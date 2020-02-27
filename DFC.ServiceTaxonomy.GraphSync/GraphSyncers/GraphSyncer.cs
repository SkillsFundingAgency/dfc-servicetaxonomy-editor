using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
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

            var partQueries = await AddContentPartSyncComponents(contentType, content);

            await SyncComponentsToGraph(graphSyncPartContent, partQueries);

            return _mergeNodeCommand;
        }

        private async Task<List<Query>> AddContentPartSyncComponents(string contentType, JObject content)
        {
            // ensure graph sync part is processed first, as other part syncers (current bagpart) require the node's id value
            string graphSyncPartName = nameof(GraphSyncPart);
            var partSyncersWithGraphLookupFirst
                = _partSyncers.Where(ps => ps.PartName != graphSyncPartName)
                    .Prepend(_partSyncers.First(ps => ps.PartName == graphSyncPartName));

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);

            List<Query> partQueries = new List<Query>();

            foreach (var partSync in partSyncersWithGraphLookupFirst)
            {
                string partName = partSync.PartName ?? contentType;

                // bag part has p.Name == <<name>>, p.PartDefinition.Name == "BagPart"
                // (other non-named parts have the part name in both)
                var contentTypePartDefinitions =
                    contentTypeDefinition.Parts.Where(p => p.PartDefinition.Name == partName);

                if (!contentTypePartDefinitions.Any())
                    continue;

                foreach (var contentTypePartDefinition in contentTypePartDefinitions)
                {
                    string namedPartName = contentTypePartDefinition.Name;

                    dynamic? partContent = content[namedPartName];
                    if (partContent == null)
                        continue; //todo: throw??

                    partQueries.AddRange(await partSync.AddSyncComponents(partContent, _mergeNodeCommand,
                        _replaceRelationshipsCommand, contentTypePartDefinition));
                }
            }

            return partQueries;
        }

        private async Task SyncComponentsToGraph(dynamic graphSyncPartContent, List<Query> partQueries)
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

            // part queries have to come after the main sync queries
            queries.AddRange(partQueries);

            await _graphDatabase.RunWriteQueries(queries.ToArray());
        }
    }
}
