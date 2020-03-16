using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class DeleteGraphSyncer : IDeleteGraphSyncer
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IDeleteNodeCommand _deleteNodeCommand;
        private readonly IDeleteNodesByTypeCommand _deleteNodesByTypeCommand;
        private readonly IGraphSyncPartIdProperty _graphSyncPartIdProperty;
        private readonly ISession _session;
        private readonly ILogger<DeleteGraphSyncer> _logger;

        //todo: have as setting of activity, or graph sync content part settings
        private const string NcsPrefix = "ncs__";

        public DeleteGraphSyncer(
            IGraphDatabase graphDatabase,
            IContentDefinitionManager contentDefinitionManager,
            IDeleteNodesByTypeCommand deleteNodesByTypeCommand,
            IDeleteNodeCommand deleteNodeCommand,
            IGraphSyncPartIdProperty graphSyncPartIdProperty,
            ISession session,
            ILogger<DeleteGraphSyncer> logger)
        {
            _graphDatabase = graphDatabase;
            _contentDefinitionManager = contentDefinitionManager;
            _deleteNodeCommand = deleteNodeCommand;
            _deleteNodesByTypeCommand = deleteNodesByTypeCommand;
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
            _session = session;
            _logger = logger;
        }

        public async Task DeleteNodesByType(string nodeType)
        {
            if (string.IsNullOrWhiteSpace(nodeType))
                return;

            _deleteNodesByTypeCommand.NodeLabels = new HashSet<string> { NcsPrefix + nodeType };

            try
            {
                await _graphDatabase.Run(_deleteNodesByTypeCommand);
            }
            //TODO : specify which exceptions to handle?
            catch
            {
                //this forces a rollback of the currect OC db transaction
                _session.Cancel();
                throw;
            }

            _logger.LogInformation($"Sync: deleting all nodes of {nodeType}");
        }

        public async Task DeleteFromGraph(ContentItem contentItem)
        {
            if (contentItem.Content.GraphSyncPart == null)
                return;

            _logger.LogInformation($"Sync: deleting {contentItem.ContentType}");

            var graphSyncPartSettings = GetGraphSyncPartSettings(contentItem.ContentType);

            string nodeLabel = string.IsNullOrEmpty(graphSyncPartSettings.NodeNameTransform)
                               || graphSyncPartSettings.NodeNameTransform == "val"
                ? contentItem.ContentType
                : "todo";

            _deleteNodeCommand.NodeLabels = new HashSet<string> {nodeLabel};
            _deleteNodeCommand.IdPropertyName = _graphSyncPartIdProperty.Name;
            _deleteNodeCommand.IdPropertyValue = _graphSyncPartIdProperty.Value(contentItem.Content.GraphSyncPart);
            _deleteNodeCommand.DeleteNode = !graphSyncPartSettings.PreexistingNode;

            try
            {
                await _graphDatabase.Run(_deleteNodeCommand);
            }
            //TODO : specify which exceptions to handle?
            catch
            {
                //this forces a rollback of the currect OC db transaction
                _session.Cancel();
                throw;
            }
        }

        private GraphSyncPartSettings GetGraphSyncPartSettings(string contentType)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphSyncPart));
            return contentTypePartDefinition.GetSettings<GraphSyncPartSettings>();
        }
    }
}
