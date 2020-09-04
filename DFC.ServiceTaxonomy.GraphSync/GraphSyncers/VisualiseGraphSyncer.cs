using System;
using System.Collections.Generic;
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

        public VisualiseGraphSyncer(IContentManager contentManager, ISyncNameProvider syncNameProvider, IDescribeContentItemHelper describeContentItemHelper, IServiceProvider serviceProvider)
        {
            _contentManager = contentManager;
            _syncNameProvider = syncNameProvider;
            _describeContentItemHelper = describeContentItemHelper;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<IQuery<INodeAndOutRelationshipsAndTheirInRelationships>>> BuildVisualisationCommands(string contentItemId, IContentItemVersion contentItemVersion)
        {
            ContentItem contentItem = await _contentManager.GetAsync(contentItemId, contentItemVersion.VersionOptions);

            if (contentItem != null)
            {

                dynamic? graphSyncPartContent = contentItem.Content[nameof(GraphSyncPart)];

                _syncNameProvider.ContentType = contentItem.ContentType;
                var sourceNodeId = _syncNameProvider.GetIdPropertyValue(graphSyncPartContent, contentItemVersion);
                var sourceNodeLabels = await _syncNameProvider.NodeLabels();
                var sourceNodeIdPropertyName = _syncNameProvider.IdPropertyName();

                var rootContext = new DescribeRelationshipsContext(sourceNodeIdPropertyName, sourceNodeId, sourceNodeLabels, contentItem, _syncNameProvider, _contentManager, contentItemVersion, null, _serviceProvider, contentItem);
                rootContext.SetContentField(contentItem.Content);

                await _describeContentItemHelper.BuildRelationships(contentItem, rootContext);

                var relationships = new List<ContentItemRelationship>();
                var relationshipCommands = await _describeContentItemHelper.GetRelationshipCommands(rootContext, relationships, rootContext);

                return relationshipCommands!;
            }

            return new List<IQuery<INodeAndOutRelationshipsAndTheirInRelationships>>();
        }
    }
}
