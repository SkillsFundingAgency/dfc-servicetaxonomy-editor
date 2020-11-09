using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Model;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class VisualiseGraphSyncer : IVisualiseGraphSyncer
    {
        private readonly IContentManager _contentManager;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IDescribeContentItemHelper _describeContentItemHelper;
        private readonly IGraphCluster _neoGraphCluster;
        private readonly IServiceProvider _serviceProvider;

        public VisualiseGraphSyncer(
            IContentManager contentManager,
            ISyncNameProvider syncNameProvider,
            IDescribeContentItemHelper describeContentItemHelper,
            IGraphCluster neoGraphCluster,
            IServiceProvider serviceProvider)
        {
            _contentManager = contentManager;
            _syncNameProvider = syncNameProvider;
            _describeContentItemHelper = describeContentItemHelper;
            _neoGraphCluster = neoGraphCluster;
            _serviceProvider = serviceProvider;
        }

        //todo: if issue with data, don't just hang visualiser
        private async Task<IEnumerable<IQuery<object?>>> BuildVisualisationCommands(
            string contentItemId,
            IContentItemVersion contentItemVersion)
        {
            ContentItem? contentItem = await contentItemVersion.GetContentItem(_contentManager, contentItemId);
            if (contentItem == null)
            {
                return Enumerable.Empty<IQuery<INodeAndOutRelationshipsAndTheirInRelationships>>();
            }
            //todo: best to not use dynamic
            dynamic? graphSyncPartContent = contentItem.Content[nameof(GraphSyncPart)];

            _syncNameProvider.ContentType = contentItem.ContentType;

            string? sourceNodeId = _syncNameProvider.GetNodeIdPropertyValue(graphSyncPartContent, contentItemVersion);
            IEnumerable<string> sourceNodeLabels = await _syncNameProvider.NodeLabels();
            string sourceNodeIdPropertyName = _syncNameProvider.IdPropertyName();

            var rootContext = new DescribeRelationshipsContext(
                sourceNodeIdPropertyName, sourceNodeId, sourceNodeLabels, contentItem, _syncNameProvider,
                _contentManager, contentItemVersion, null, _serviceProvider, contentItem);
            rootContext.SetContentField(contentItem.Content);

            await _describeContentItemHelper.BuildRelationships(contentItem, rootContext);

            //todo: return relationships - can we do it without creating cypher outside of a query?
            var relationships = new List<ContentItemRelationship>();
            return await _describeContentItemHelper.GetRelationshipCommands(rootContext, relationships, rootContext);
        }

        public async Task<Subgraph> GetVisualisationSubgraph(
            string contentItemId,
            string graphName,
            IContentItemVersion contentItemVersion)
        {
            var relationshipCommands = await BuildVisualisationCommands(contentItemId, contentItemVersion!);

            // get all results atomically
            var result = await _neoGraphCluster.Run(graphName, relationshipCommands.ToArray());

            var inAndOutResults =
                result.OfType<INodeAndOutRelationshipsAndTheirInRelationships?>();

            //todo: should really always return the source node (until then, the subgraph will pull it if the main results don't)
            Subgraph subgraph;
            if (inAndOutResults.Any())
            {
                // get all outgoing relationships from the query and add in any source nodes

                subgraph = new Subgraph(
                    inAndOutResults
                        .SelectMany(x => x!.OutgoingRelationships.Select(x => x.outgoingRelationship.DestinationNode))
                        .Union(inAndOutResults.GroupBy(x => x!.SourceNode).Select(z => z.FirstOrDefault()!.SourceNode)),
                    inAndOutResults!
                        .SelectMany(y => y!.OutgoingRelationships.Select(z => z.outgoingRelationship.Relationship))
                        .ToHashSet(),
                    inAndOutResults.FirstOrDefault()?.SourceNode);
            }
            else
            {
                subgraph = new Subgraph();
            }

            ISubgraph? inResults = result.OfType<ISubgraph>().FirstOrDefault();
            if (inResults != null)
            {
                subgraph.Add(inResults);
            }

            return subgraph;
        }
    }
}
