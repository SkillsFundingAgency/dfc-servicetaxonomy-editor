
using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IDescribeRelationshipsContext : IGraphSyncContext
    {
        IServiceProvider ServiceProvider { get; set; }

        List<ContentItemRelationship> AvailableRelationships { get; set; }

        JObject? ContentField { get; }

        string SourceNodeId { get; set; }

        IEnumerable<string> SourceNodeLabels { get; set; }

        int CurrentDepth { get; set; }

        string SourceNodeIdPropertyName { get; set; }

        void SetContentField(JObject jObject);

        new ContentTypePartDefinition ContentTypePartDefinition { get; set; }

        ContentItem RootContentItem { get; set; }
    }
}
