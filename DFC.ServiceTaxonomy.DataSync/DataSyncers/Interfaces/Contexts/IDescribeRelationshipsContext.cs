using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts
{
    public interface IDescribeRelationshipsContext : IDataSyncContext
    {
        IServiceProvider ServiceProvider { get; }

        List<ContentItemRelationship> AvailableRelationships { get; }

        string SourceNodeId { get; }

        IEnumerable<string> SourceNodeLabels { get; }

        int CurrentDepth { get; }
        int MaxDepthFromHere { get; }

        string SourceNodeIdPropertyName { get; }
    }
}
