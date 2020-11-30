﻿using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers
{
    public interface IContentFieldsGraphSyncer
    {
        Task AllowSync(JObject content, IGraphMergeContext context, IAllowSync allowSync)
            => Task.CompletedTask;

        Task AddSyncComponents(JObject content, IGraphMergeContext context);

        Task AddRelationship(JObject content, IDescribeRelationshipsContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context);
    }
}
