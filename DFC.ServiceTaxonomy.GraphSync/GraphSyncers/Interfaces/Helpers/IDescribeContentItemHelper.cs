using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.Models;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers
{
    public interface IDescribeContentItemHelper
    {
        Task BuildRelationships(ContentItem contentItem, IDescribeRelationshipsContext context, string sourceNodeIdPropertyName, string sourceNodeId, IEnumerable<string> sourceNodeLabels);
        Task<IEnumerable<string?>> GetRelationshipCommands(IDescribeRelationshipsContext context, List<ContentItemRelationship> currentList, IDescribeRelationshipsContext parentContext);
        void SetRootContentItem(ContentItem contentItem);
    }
}
