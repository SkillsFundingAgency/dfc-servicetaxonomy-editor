using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class DescribeContentItemHelper : IDescribeContentItemHelper
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISyncNameProvider _graphSyncHelper;
        private readonly IEnumerable<IContentPartGraphSyncer> _contentPartGraphSyncers;
        private readonly IEnumerable<IContentFieldGraphSyncer> _contentFieldsGraphSyncers;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IServiceProvider _serviceProvider;

        public ContentItem? RootContentItem { get; private set; }

        public DescribeContentItemHelper(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ISyncNameProvider graphSyncHelper,
            IEnumerable<IContentPartGraphSyncer> contentPartGraphSyncers,
            IEnumerable<IContentFieldGraphSyncer> contentFieldsGraphSyncers,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IServiceProvider serviceProvider)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _graphSyncHelper = graphSyncHelper;
            _contentPartGraphSyncers = contentPartGraphSyncers;
            _contentFieldsGraphSyncers = contentFieldsGraphSyncers;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _serviceProvider = serviceProvider;
        }

        public void SetRootContentItem(ContentItem contentItem)
        {
            RootContentItem = contentItem;
        }

        public async Task<IEnumerable<string?>> GetRelationshipCommands(IDescribeRelationshipsContext context, List<ContentItemRelationship> currentList, IDescribeRelationshipsContext parentContext)
        {
            var allRelationships = await GetRelationships(context, currentList, parentContext);
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

            List<string> commandsToReturn = BuildOutgoingRelationshipCommands(uniqueCommands);
            BuildIncomingRelationshipCommands(commandsToReturn, context);

            return commandsToReturn;
        }

        private void BuildIncomingRelationshipCommands(List<string> commandsToReturn, IDescribeRelationshipsContext context)
        {
            var commandStringBuilder = new StringBuilder($"match (s)-[r]->(d:{string.Join(":", context.SourceNodeLabels)} {{{context.SourceNodeIdPropertyName}: '{context.SourceNodeId}'}})");
            commandStringBuilder.AppendLine(" with s, {destNode: d, relationship: r, destinationIncomingRelationships:collect({destIncomingRelationship:'todo',  destIncomingRelSource:'todo'})} as relationshipDetails");
            commandStringBuilder.AppendLine(" with { sourceNode: s, outgoingRelationships: collect(relationshipDetails)} as nodeAndOutRelationshipsAndTheirInRelationships");
            commandStringBuilder.AppendLine(" return nodeAndOutRelationshipsAndTheirInRelationships");

            commandsToReturn.Add(commandStringBuilder.ToString());
        }

        private static List<string> BuildOutgoingRelationshipCommands(IEnumerable<string?> uniqueCommands)
        {
            var commandsToReturn = new List<string>();

            foreach (var command in uniqueCommands.ToList())
            {
                var withStringBuilder = new StringBuilder();

                int currentDepth = 1;
                int depthCount = Regex.Matches(command, "d[0-9]+:").Count;

                List<string> collectClauses = new List<string>();
                List<string> relationshipClauses = new List<string>();
                while (currentDepth <= depthCount)
                {
                    relationshipClauses.Add($"{{destNode: d{currentDepth}, relationship: r{currentDepth}, destinationIncomingRelationships:collect({{destIncomingRelationship:'todo',  destIncomingRelSource:'todo'}})}} as dr{currentDepth}RelationshipDetails");

                    collectClauses.Add($"collect(dr{currentDepth}RelationshipDetails)");
                    currentDepth++;
                }

                withStringBuilder.AppendLine($"with s,{string.Join(',', relationshipClauses)}");
                withStringBuilder.AppendLine($"with {{sourceNode: s, outgoingRelationships: {string.Join('+', collectClauses)}}} as nodeAndOutRelationshipsAndTheirInRelationships");
                withStringBuilder.AppendLine("return nodeAndOutRelationshipsAndTheirInRelationships");
                commandsToReturn.Add($"{command} {withStringBuilder}");
            }

            return commandsToReturn;
        }

        private async Task<IEnumerable<ContentItemRelationship>> GetRelationships(IDescribeRelationshipsContext context, List<ContentItemRelationship> currentList, IDescribeRelationshipsContext parentContext)
        {
            if (context.AvailableRelationships != null)
            {
                foreach (var child in context.AvailableRelationships)
                {
                    if (child != null)
                    {
                        context.CurrentDepth = parentContext.CurrentDepth + 1;
                        var parentRelationship = parentContext.AvailableRelationships.FirstOrDefault(x => x.Destination.All(child.Source.Contains));

                        if (parentRelationship != null && !string.IsNullOrEmpty(parentRelationship.RelationshipPathString))
                        {
                            var relationshipString = $"{parentRelationship.RelationshipPathString}-[r{context.CurrentDepth}:{child.Relationship}]-(d{context.CurrentDepth}:{string.Join(":", child.Destination!)})";
                            child.RelationshipPathString = relationshipString;
                        }
                        else
                        {
                            context.CurrentDepth = 1;
                            child.RelationshipPathString = $@"match (s:{string.Join(":", context.SourceNodeLabels)} {{{context.SourceNodeIdPropertyName}: '{context.SourceNodeId}'}})-[r{1}:{child.Relationship}]-(d{1}:{string.Join(":", child.Destination!)})";
                        }
                    }
                }

                currentList.AddRange(context.AvailableRelationships);
            }

            foreach (var childContext in context.ChildContexts)
            {
                await GetRelationships((IDescribeRelationshipsContext)childContext, currentList, context);
            }

            return currentList;
        }

        public async Task BuildRelationships(ContentItem contentItem, IDescribeRelationshipsContext context, string sourceNodeIdPropertyName, string sourceNodeId, IEnumerable<string> sourceNodeLabels)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            _graphSyncHelper.ContentType = contentItem.ContentType;

            var itemContext = contentItem == RootContentItem ? context : new DescribeRelationshipsContext(sourceNodeIdPropertyName, sourceNodeId, sourceNodeLabels, contentItem, _graphSyncHelper, _contentManager, _publishedContentItemVersion, context, _serviceProvider);

            foreach (var part in contentTypeDefinition.Parts)
            {
                var partSyncer = _contentPartGraphSyncers.FirstOrDefault(x => x.PartName == part.Name);

                if (partSyncer != null)
                {
                    await partSyncer.AddRelationship(itemContext);
                }

                foreach (var relationshipField in part.PartDefinition.Fields)
                {
                    itemContext.SetContentPartFieldDefinition(relationshipField);
                    itemContext.SetContentField((JObject)contentItem.Content[relationshipField.PartDefinition.Name][relationshipField.Name]);

                    var fieldSyncer = _contentFieldsGraphSyncers.FirstOrDefault(x => x.FieldTypeName == relationshipField.FieldDefinition.Name);

                    var contentItemIds =
                           (JArray)contentItem.Content[relationshipField.PartDefinition.Name][relationshipField.Name]
                               .ContentItemIds;

                    if (contentItemIds != null)
                    {
                        foreach (var relatedContentItemId in contentItemIds)
                        {
                            await BuildRelationships(await _contentManager.GetAsync(relatedContentItemId.ToString()), itemContext, sourceNodeIdPropertyName, sourceNodeId, sourceNodeLabels);
                        }
                    }

                    if (fieldSyncer != null)
                    {
                        await fieldSyncer.AddRelationship(itemContext);
                    }
                }
            }

            if (contentItem != RootContentItem)
            {
                context.AddChildContext(itemContext);
            }
        }
    }
}
