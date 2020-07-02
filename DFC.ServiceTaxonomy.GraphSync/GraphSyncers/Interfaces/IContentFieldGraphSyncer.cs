﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentFieldGraphSyncer
    {
        string FieldTypeName {get;}

        Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IContentManager contentManager,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts);
    }
}
