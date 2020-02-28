using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    //todo: have to refactor sync. currently with bags, a single sync will occur in multiple transactions
    // fo if a validation fails for example, the graph will be left in an incomplete synced state
    // need to gather up all commands, then execute them in a single transaction
    // giving the commands the opportunity to validate the results before the transaction is committed
    // so any validation failure rolls back the whole sync operation
    public class GraphSyncer : IGraphSyncer
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartGraphSyncer> _partSyncers;
        private readonly IGraphSyncPartIdProperty _graphSyncPartIdProperty;
        private readonly IMergeNodeCommand _mergeNodeCommand;
        private readonly IReplaceRelationshipsCommand _replaceRelationshipsCommand;
        private readonly ILogger<GraphSyncer> _logger;

        public GraphSyncer(
            IGraphDatabase graphDatabase,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPartGraphSyncer> partSyncers,
            IGraphSyncPartIdProperty graphSyncPartIdProperty,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ILogger<GraphSyncer> logger)
        {
            _graphDatabase = graphDatabase;
            _contentDefinitionManager = contentDefinitionManager;
            _partSyncers = partSyncers;
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
            _mergeNodeCommand = mergeNodeCommand;
            _replaceRelationshipsCommand = replaceRelationshipsCommand;
            _logger = logger;
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

            _logger.LogInformation($"Syncing {contentType}");

            // could inject _graphSyncPartIdProperty into mergeNodeCommand, but should we?

            _mergeNodeCommand.NodeLabels.Add(NcsPrefix + contentType);
            _mergeNodeCommand.NodeLabels.Add(CommonNodeLabel);
            _mergeNodeCommand.IdPropertyName = _graphSyncPartIdProperty.Name;

            List<ICommand> partCommands = await AddContentPartSyncComponents(contentType, content);

            await SyncComponentsToGraph(graphSyncPartContent, partCommands);

            return _mergeNodeCommand;
        }

        private async Task<List<ICommand>> AddContentPartSyncComponents(string contentType, JObject content)
        {
            // ensure graph sync part is processed first, as other part syncers (current bagpart) require the node's id value
            string graphSyncPartName = nameof(GraphSyncPart);
            var partSyncersWithGraphLookupFirst
                = _partSyncers.Where(ps => ps.PartName != graphSyncPartName)
                    .Prepend(_partSyncers.First(ps => ps.PartName == graphSyncPartName));

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);

            List<ICommand> partCommands = new List<ICommand>();

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

                    partCommands.AddRange(await partSync.AddSyncComponents(partContent, _mergeNodeCommand,
                        _replaceRelationshipsCommand, contentTypePartDefinition));
                }
            }

            return partCommands;
        }

        private async Task SyncComponentsToGraph(dynamic graphSyncPartContent, List<ICommand> partCommands)
        {
            List<ICommand> commands  = new List<ICommand> {_mergeNodeCommand};

            if (_replaceRelationshipsCommand.Relationships.Any())
            {
                // doesn't really belong here...
                _replaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(_mergeNodeCommand.NodeLabels);
                _replaceRelationshipsCommand.SourceIdPropertyName = _mergeNodeCommand.IdPropertyName;
                _replaceRelationshipsCommand.SourceIdPropertyValue = _graphSyncPartIdProperty.Value(graphSyncPartContent);

                commands.Add(_replaceRelationshipsCommand);
            }

            // part queries have to come after the main sync queries
            commands.AddRange(partCommands);

            var results = await _graphDatabase.RunWriteQueries(commands.Select(c => c.Query).ToArray());

            int currentResultIndex = -1;
            foreach (ICommand command in commands)
            {
                var currentResult = results[++currentResultIndex];
                command.ValidateResults(currentResult.records, currentResult.resultSummary);
            }
        }
    }
}
