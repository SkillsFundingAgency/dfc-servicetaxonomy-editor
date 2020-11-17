using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Models;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IDescribeRelationshipsContext : IGraphSyncContext
    {
        IServiceProvider ServiceProvider { get; }

        List<ContentItemRelationship> AvailableRelationships { get; }

        string SourceNodeId { get; }

        IEnumerable<string> SourceNodeLabels { get; }

        int CurrentDepth { get; set; }

        string SourceNodeIdPropertyName { get; }

        ContentItem RootContentItem { get; }
    }
}
