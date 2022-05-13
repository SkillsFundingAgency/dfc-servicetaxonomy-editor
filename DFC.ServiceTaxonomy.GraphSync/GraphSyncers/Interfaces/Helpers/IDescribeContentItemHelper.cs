using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers
{
    public interface IDescribeContentItemHelper
    {
        Task<IDescribeRelationshipsContext?> BuildRelationships(
            ContentItem contentItem,
            string sourceNodeIdPropertyName,
            string sourceNodeId,
            IEnumerable<string> sourceNodeLabels,
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IDescribeRelationshipsContext? parentContext,
            IServiceProvider serviceProvider);

        Task<IDescribeRelationshipsContext?> BuildRelationships(string contentItemId, IDescribeRelationshipsContext context);
        Task<IEnumerable<IQuery<object?>>> GetRelationshipCommands(IDescribeRelationshipsContext context);//, IDescribeRelationshipsContext parentContext);
    }
}
