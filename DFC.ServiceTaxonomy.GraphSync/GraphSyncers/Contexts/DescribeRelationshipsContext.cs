using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class DescribeRelationshipsContext : GraphSyncContext, IDescribeRelationshipsContext
    {
        public new IDescribeRelationshipsContext? ParentContext { get; }
        public new IEnumerable<IDescribeRelationshipsContext> ChildContexts => _childContexts.Cast<IDescribeRelationshipsContext>();
        public List<ContentItemRelationship> AvailableRelationships { get; set; }

        public IServiceProvider ServiceProvider { get; set; }
        public JObject? ContentField { get; private set; }

        public DescribeRelationshipsContext(
            ContentItem contentItem,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IDescribeRelationshipsContext? parentContext,
            IServiceProvider serviceProvider) : base(contentItem, graphSyncHelper, contentManager, contentItemVersion, parentContext, serviceProvider.GetRequiredService<ILogger<GraphDeleteContext>>())
        {
            AvailableRelationships = new List<ContentItemRelationship>();
            ServiceProvider = serviceProvider;
        }

        public void SetContentField(JObject jObject)
        {
            ContentField = jObject;
        }
    }
}

