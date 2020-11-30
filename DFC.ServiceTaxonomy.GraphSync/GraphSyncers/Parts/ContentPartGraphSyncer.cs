﻿using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public abstract class ContentPartGraphSyncer : IContentPartGraphSyncer
    {
        public virtual int Priority => 0;
        public abstract string PartName { get; }

        public virtual bool CanSync(string contentType, ContentPartDefinition contentPartDefinition)
        {
            return contentPartDefinition.Name == PartName;
        }

        public virtual Task AllowSync(JObject content, IGraphMergeContext context, IAllowSync allowSync)
        {
            return Task.CompletedTask;
        }

        public abstract Task AddSyncComponents(JObject content, IGraphMergeContext context);

        public virtual Task AllowSyncDetaching(IGraphMergeContext context, IAllowSync allowSync)
        {
            return Task.CompletedTask;
        }

        public virtual Task AddSyncComponentsDetaching(IGraphMergeContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task AllowDelete(JObject content, IGraphDeleteContext context, IAllowSync allowSync)
        {
            return Task.CompletedTask;
        }

        public virtual Task DeleteComponents(JObject content, IGraphDeleteContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task MutateOnClone(JObject content, ICloneContext context)
        {
            return Task.CompletedTask;
        }

        public abstract Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context);

        public virtual Task AddRelationship(JObject content, IDescribeRelationshipsContext context)
        {
            return Task.CompletedTask;
        }
    }
}
