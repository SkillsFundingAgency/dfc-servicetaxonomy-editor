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

        // public string? SourceNodeId { get; private set; }
        // public IEnumerable<string>? SourceNodeLabels { get; private set; }
        // public string? SourceNodeIdPropertyName { get; private set; }

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

//        public async Task<IEnumerable<IQuery<INodeAndOutRelationshipsAndTheirInRelationships>>> BuildVisualisationCommands(string contentItemId, IContentItemVersion contentItemVersion)
        private async Task<IEnumerable<IQuery<object?>>> BuildVisualisationCommands(string contentItemId, IContentItemVersion contentItemVersion)
        {
            ContentItem? contentItem = await contentItemVersion.GetContentItem(_contentManager, contentItemId);
            if (contentItem == null)
            {
                return Enumerable.Empty<IQuery<INodeAndOutRelationshipsAndTheirInRelationships>>();
            }

            dynamic? graphSyncPartContent = contentItem.Content[nameof(GraphSyncPart)];

            _syncNameProvider.ContentType = contentItem.ContentType;

            string? SourceNodeId = _syncNameProvider.GetIdPropertyValue(graphSyncPartContent, contentItemVersion);
            IEnumerable<string>? SourceNodeLabels = await _syncNameProvider.NodeLabels();
            string? SourceNodeIdPropertyName = _syncNameProvider.IdPropertyName();

            var rootContext = new DescribeRelationshipsContext(SourceNodeIdPropertyName, SourceNodeId, SourceNodeLabels, contentItem, _syncNameProvider, _contentManager, contentItemVersion, null, _serviceProvider, contentItem);
            rootContext.SetContentField(contentItem.Content);

            await _describeContentItemHelper.BuildRelationships(contentItem, rootContext);

            var relationships = new List<ContentItemRelationship>();
            return await _describeContentItemHelper.GetRelationshipCommands(rootContext, relationships, rootContext);
        }

        public async Task<Subgraph> GetData(string contentItemId, string graphName, IContentItemVersion contentItemVersion)
        {
            var relationshipCommands = await BuildVisualisationCommands(contentItemId, contentItemVersion!);

            // get all results atomically
            var result = await _neoGraphCluster.Run(graphName, relationshipCommands.ToArray());

            // string owlResponseString = "";
            // IEnumerable<INode> nodesToProcess = new List<INode>();
            // long sourceNodeId = 0;
            // HashSet<IRelationship> relationships = new HashSet<IRelationship>();

            var data = new Subgraph();

            var inAndOutResults =
                result.OfType<INodeAndOutRelationshipsAndTheirInRelationships?>();

            if (inAndOutResults.Any())
            {
                //Get all outgoing relationships from the query and add in any source nodes
                data.Nodes.UnionWith(
                    inAndOutResults
                    .SelectMany(x => x!.OutgoingRelationships.Select(x => x.outgoingRelationship.DestinationNode))
                    .Union(inAndOutResults.GroupBy(x => x!.SourceNode).Select(z => z.FirstOrDefault()!.SourceNode)));
                data.SelectedNodeId = inAndOutResults.FirstOrDefault()!.SourceNode.Id;
                data.Relationships.UnionWith(
                    inAndOutResults!
                        .SelectMany(y => y!.OutgoingRelationships.Select(z => z.outgoingRelationship.Relationship))
                    .ToHashSet());
            }

            var inResults = result.OfType<ISubgraph>().FirstOrDefault();
            if (inResults != null)
            {
                data.Add(inResults);
            }

            return data;

            //todo: when do we get no results?
            // else
            // {
            //     var nodeOnlyResult = await _neoGraphCluster.Run(graph, new NodeWithOutgoingRelationshipsQuery(_visualiseGraphSyncer.SourceNodeLabels!, _visualiseGraphSyncer.SourceNodeIdPropertyName!, _visualiseGraphSyncer.SourceNodeId!));
            //     nodesToProcess = nodeOnlyResult.GroupBy(x => x!.SourceNode).Select(z => z.FirstOrDefault()!.SourceNode);
            //     sourceNodeId = nodeOnlyResult.FirstOrDefault()!.SourceNode.Id;
            //     relationships = nodeOnlyResult!.SelectMany(y => y!.OutgoingRelationships.Select(z => z.Relationship)).ToHashSet<IRelationship>();
            // }

            // var owlDataModel = _neo4JToOwlGeneratorService.CreateOwlDataModels(sourceNodeId, nodesToProcess, relationships, "skos__prefLabel");
            // owlResponseString = JsonSerializer.Serialize(owlDataModel, _jsonOptions);
            // return Content(owlResponseString, MediaTypeNames.Application.Json);
        }
    }
}
