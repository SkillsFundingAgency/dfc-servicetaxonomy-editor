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
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class VisualiseGraphSyncer : IVisualiseGraphSyncer
    {
        private readonly IContentManager _contentManager;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IDescribeContentItemHelper _describeContentItemHelper;
        private readonly IServiceProvider _serviceProvider;

        public string? SourceNodeId { get; private set; }
        public IEnumerable<string>? SourceNodeLabels { get; private set; }
        public string? SourceNodeIdPropertyName { get; private set; }

        public VisualiseGraphSyncer(IContentManager contentManager, ISyncNameProvider syncNameProvider, IDescribeContentItemHelper describeContentItemHelper, IServiceProvider serviceProvider)
        {
            _contentManager = contentManager;
            _syncNameProvider = syncNameProvider;
            _describeContentItemHelper = describeContentItemHelper;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<IQuery<INodeAndOutRelationshipsAndTheirInRelationships>>> BuildVisualisationCommands(string contentItemId, IContentItemVersion contentItemVersion)
        {
            ContentItem? contentItem = await contentItemVersion.GetContentItem(_contentManager, contentItemId);
            if (contentItem == null)
            {
                return Enumerable.Empty<IQuery<INodeAndOutRelationshipsAndTheirInRelationships>>();
            }

            dynamic? graphSyncPartContent = contentItem.Content[nameof(GraphSyncPart)];

            _syncNameProvider.ContentType = contentItem.ContentType;

            SourceNodeId = _syncNameProvider.GetIdPropertyValue(graphSyncPartContent, contentItemVersion);
            SourceNodeLabels = await _syncNameProvider.NodeLabels();
            SourceNodeIdPropertyName = _syncNameProvider.IdPropertyName();

            var rootContext = new DescribeRelationshipsContext(SourceNodeIdPropertyName, SourceNodeId, SourceNodeLabels, contentItem, _syncNameProvider, _contentManager, contentItemVersion, null, _serviceProvider, contentItem);
            rootContext.SetContentField(contentItem.Content);

            await _describeContentItemHelper.BuildRelationships(contentItem, rootContext);

            var relationships = new List<ContentItemRelationship>();
            var relationshipCommands = await _describeContentItemHelper.GetRelationshipCommands(rootContext, relationships, rootContext);

            return relationshipCommands!;
        }
    }
}
