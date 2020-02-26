using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class BagPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IServiceProvider _serviceProvider;

        public string? PartName => nameof(BagPart);

        public BagPartGraphSyncer(IContentDefinitionManager contentDefinitionManager, IServiceProvider serviceProvider)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<Query>> AddSyncComponents(
            dynamic graphLookupContent,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            var queries = new List<Query>();

            foreach (JObject? contentItem in graphLookupContent.ContentItems)
            {
                var graphSyncer = _serviceProvider.GetRequiredService<IGraphSyncer>();

                string contentType = contentItem!["ContentType"]!.ToString();
                //todo: if we want to support nested bags, would have to return queries also
                IMergeNodeCommand? containedContentMergeNodeCommand = await graphSyncer.SyncToGraph(contentType, contentItem!);
                // only sync the content type contained in the bag if it has a graph lookup part
                if (containedContentMergeNodeCommand == null)
                    continue;

                var delayedReplaceRelationshipsCommand = _serviceProvider.GetRequiredService<IReplaceRelationshipsCommand>();
                delayedReplaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(mergeNodeCommand.NodeLabels);

                if (mergeNodeCommand.IdPropertyName == null)
                    throw new GraphSyncException($"Supplied merge node command has null {nameof(mergeNodeCommand.IdPropertyName)}");
                delayedReplaceRelationshipsCommand.SourceIdPropertyName = mergeNodeCommand.IdPropertyName;
                delayedReplaceRelationshipsCommand.SourceIdPropertyValue = (string?)mergeNodeCommand.Properties[delayedReplaceRelationshipsCommand.SourceIdPropertyName];

                var graphSyncPartSettings = GetGraphSyncPartSettings(contentType);
                string? relationshipType = graphSyncPartSettings.BagPartContentItemRelationshipType;
                if (string.IsNullOrEmpty(relationshipType))
                    relationshipType = "ncs__hasBagPart";

                //todo: the types should match. string[] to object[]?

                if (containedContentMergeNodeCommand.IdPropertyName == null)
                    throw new GraphSyncException($"Merge node command from bag contained content has null {nameof(containedContentMergeNodeCommand.IdPropertyName)}");

                delayedReplaceRelationshipsCommand.AddRelationshipsTo(
                    relationshipType,
                    containedContentMergeNodeCommand.NodeLabels,
                    containedContentMergeNodeCommand.IdPropertyName,
                    (string)containedContentMergeNodeCommand.Properties[containedContentMergeNodeCommand.IdPropertyName]);

                queries.Add(delayedReplaceRelationshipsCommand.Query);
            }

            return queries;
        }

        private GraphSyncPartSettings GetGraphSyncPartSettings(string contentType)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphSyncPart));
            return contentTypePartDefinition.GetSettings<GraphSyncPartSettings>();
        }
    }
}
