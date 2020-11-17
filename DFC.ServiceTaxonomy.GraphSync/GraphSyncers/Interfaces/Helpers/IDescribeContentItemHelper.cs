using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers
{
    public interface IDescribeContentItemHelper
    {
        Task BuildRelationships(ContentItem contentItem, IDescribeRelationshipsItemSyncContext context);
        Task BuildRelationships(string contentItemId, IDescribeRelationshipsContext context);
        Task<IEnumerable<IQuery<object?>>> GetRelationshipCommands(IDescribeRelationshipsContext context, List<ContentItemRelationship> currentList, IDescribeRelationshipsContext parentContext);
    }
}
