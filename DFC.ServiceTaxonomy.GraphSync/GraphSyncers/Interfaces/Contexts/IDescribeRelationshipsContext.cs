
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

        public string SourceNodeId { get; set; }

        public IEnumerable<string> SourceNodeLabels { get; set; }

        public int CurrentDepth { get; set; }

        public string SourceNodeIdPropertyName { get; set; }

        void SetContentField(JObject jObject);
    }
}
