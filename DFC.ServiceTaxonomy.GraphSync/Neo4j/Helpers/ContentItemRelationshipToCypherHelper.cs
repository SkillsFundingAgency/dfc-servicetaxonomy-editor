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
        public static async Task<IEnumerable<ContentItemRelationship>> GetRelationships(IDescribeRelationshipsContext context, List<ContentItemRelationship> currentList, IDescribeRelationshipsContext parentContext)
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
    }
}
