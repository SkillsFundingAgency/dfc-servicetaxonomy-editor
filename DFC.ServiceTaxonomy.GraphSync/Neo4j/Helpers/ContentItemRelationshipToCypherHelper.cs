using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Helpers
{
    public static class ContentItemRelationshipToCypherHelper
    {
        internal static async Task<IEnumerable<ContentItemRelationship>> GetRelationships(IDescribeRelationshipsContext context, List<ContentItemRelationship> currentList, IDescribeRelationshipsContext parentContext, int? maxVisualiserDepth = null)
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

            if (maxVisualiserDepth != null && context.CurrentDepth >= maxVisualiserDepth.Value)
            {
                return currentList;
            }

            foreach (var childContext in context.ChildContexts)
            {
                var graphSyncPartSettings = context.SyncNameProvider.GetGraphSyncPartSettings(childContext.ContentItem.ContentType);

                int? childMaxDepth = maxVisualiserDepth;

                if (graphSyncPartSettings?.VisualiserNodeDepth != null && graphSyncPartSettings.VisualiserNodeDepth.Value < maxVisualiserDepth &&
                    maxVisualiserDepth - context.CurrentDepth >= context.CurrentDepth + graphSyncPartSettings.VisualiserNodeDepth)
                {
                        childMaxDepth = context.CurrentDepth + graphSyncPartSettings.VisualiserNodeDepth;
                }

                await GetRelationships((IDescribeRelationshipsContext)childContext, currentList, context, childMaxDepth);
            }

            return currentList;
        }
    }
}
