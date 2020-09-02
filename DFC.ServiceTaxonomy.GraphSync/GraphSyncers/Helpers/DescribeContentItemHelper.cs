using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class DescribeContentItemHelper : IDescribeContentItemHelper
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartGraphSyncer> _contentPartGraphSyncers;
        private readonly List<string> encounteredContentItems = new List<string>();

        public DescribeContentItemHelper(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ISyncNameProvider syncNameProvider,
            IEnumerable<IContentPartGraphSyncer> contentPartGraphSyncers,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IServiceProvider serviceProvider)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentPartGraphSyncers = contentPartGraphSyncers;
        }

        public async Task<IEnumerable<IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>>> GetRelationshipCommands(IDescribeRelationshipsContext context, List<ContentItemRelationship> currentList, IDescribeRelationshipsContext parentContext)
        {
            var allRelationships = await ContentItemRelationshipToCypherHelper.GetRelationships(context, currentList, parentContext);
            var groupedCommands = allRelationships.Select(z => z.RelationshipPathString).GroupBy(x => x).Select(g => g.First());
            var uniqueCommands = new List<string>();

            //Remove any commands containing other commands from the call structure
            foreach (var command in groupedCommands)
            {
                if (command == null)
                {
                    continue;
                }

                if (!groupedCommands.Any(z => z!.Contains(command) && z.Length > command.Length))
                {
                    uniqueCommands.Add(command);
                }
            }

            List<IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>> commandsToReturn = BuildOutgoingRelationshipCommands(uniqueCommands);
            BuildIncomingRelationshipCommands(commandsToReturn, context);

            return commandsToReturn;
        }

        private void BuildIncomingRelationshipCommands(List<IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>> commandsToReturn, IDescribeRelationshipsContext context)
        {
            commandsToReturn.Add(new NodeAndIncomingRelationshipsQuery(context.SourceNodeLabels, context.SourceNodeIdPropertyName, context.SourceNodeId));
        }

        private static List<IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>> BuildOutgoingRelationshipCommands(IEnumerable<string?> uniqueCommands)
        {
            var commandsToReturn = new List<IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>>();

            foreach (var command in uniqueCommands.ToList())
            {
                commandsToReturn.Add(new NodeAndNestedOutgoingRelationshipsQuery(command!));
            }

            return commandsToReturn;
        }

        

        public async Task BuildRelationships(string contentItemId, IDescribeRelationshipsContext context, string sourceNodeIdPropertyName, string sourceNodeId, IEnumerable<string> sourceNodeLabels)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, context.ContentItemVersion.VersionOptions);
            var childContext = new DescribeRelationshipsContext(context.SourceNodeIdPropertyName, context.SourceNodeId, context.SourceNodeLabels, contentItem, context.SyncNameProvider, context.ContentManager, context.ContentItemVersion, context, context.ServiceProvider, context.RootContentItem);

            context.AddChildContext(childContext);

            await BuildRelationships(contentItem, childContext, sourceNodeIdPropertyName, sourceNodeId, sourceNodeLabels);
        }

        public async Task BuildRelationships(ContentItem contentItem, IDescribeRelationshipsContext context, string sourceNodeIdPropertyName, string sourceNodeId, IEnumerable<string> sourceNodeLabels)
        {
            if (encounteredContentItems.Any(x => x == contentItem.ContentItemId))
            {
                return;
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            foreach (var partSync in _contentPartGraphSyncers)
            {
                foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
                {
                    string namedPartName = contentTypePartDefinition.Name;

                    JObject? partContent = contentItem.Content[namedPartName];
                    if (partContent == null)
                        continue;

                    context.ContentTypePartDefinition = contentTypePartDefinition;

                    context.SetContentField(partContent);
                    await partSync.AddRelationship(context);
                }
            }

            encounteredContentItems.Add(contentItem.ContentItemId);
        }
    }
}
