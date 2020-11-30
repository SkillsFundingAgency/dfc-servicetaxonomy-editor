﻿using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class DescribeRelationshipsContext : GraphSyncContext, IDescribeRelationshipsItemSyncContext
    {
        public new IDescribeRelationshipsContext? ParentContext { get; }
        public new IEnumerable<IDescribeRelationshipsContext> ChildContexts => _childContexts.Cast<IDescribeRelationshipsContext>();
        public List<ContentItemRelationship> AvailableRelationships { get; set; }

        public IServiceProvider ServiceProvider { get; }
        public int CurrentDepth { get; }
        public int MaxDepthFromHere { get; }
        public string SourceNodeId { get; }
        public string SourceNodeIdPropertyName { get; }
        public IEnumerable<string> SourceNodeLabels { get; }

        public DescribeRelationshipsContext(string sourceNodeIdPropertyName,
            string sourceNodeId,
            IEnumerable<string> sourceNodeLabels,
            ContentItem contentItem,
            int maxDepthFromHere,
            ISyncNameProvider graphSyncHelper,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IDescribeRelationshipsContext? parentContext,
            IServiceProvider serviceProvider) : base(
                contentItem,
                graphSyncHelper,
                contentManager,
                contentItemVersion,
                parentContext,
                serviceProvider.GetRequiredService<ILogger<GraphDeleteContext>>())
        {
            AvailableRelationships = new List<ContentItemRelationship>();
            ServiceProvider = serviceProvider;
            SourceNodeId = sourceNodeId;
            SourceNodeLabels = sourceNodeLabels;
            MaxDepthFromHere = maxDepthFromHere;
            SourceNodeIdPropertyName = sourceNodeIdPropertyName;
            CurrentDepth = (parentContext?.CurrentDepth + 1) ?? 0;
        }
    }
}

