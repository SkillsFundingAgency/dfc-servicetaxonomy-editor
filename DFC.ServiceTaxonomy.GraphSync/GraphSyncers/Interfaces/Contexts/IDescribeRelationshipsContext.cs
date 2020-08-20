
using System;
using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IDescribeRelationshipsContext : IGraphSyncContext
    {
        public IServiceProvider ServiceProvider { get; set; }

        public List<string> AvailableRelationships { get; set; }
    }
}
