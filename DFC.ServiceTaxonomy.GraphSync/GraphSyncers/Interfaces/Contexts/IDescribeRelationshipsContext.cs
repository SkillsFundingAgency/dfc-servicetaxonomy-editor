
using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IDescribeRelationshipsContext : IGraphSyncContext
    {
        public IServiceProvider ServiceProvider { get; set; }

        public List<ContentItemRelationship> AvailableRelationships { get; set; }

        public JObject? ContentField { get; }

        void SetContentField(JObject jObject);
    }
}
