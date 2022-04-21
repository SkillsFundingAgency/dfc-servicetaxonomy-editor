using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts
{
    public abstract class ContentPartDataSyncer : IContentPartDataSyncer
    {
        public virtual int Priority => 0;
        public abstract string PartName { get; }

        public virtual bool CanSync(string contentType, ContentPartDefinition contentPartDefinition)
        {
            return contentPartDefinition.Name == PartName;
        }

        public virtual Task AllowSync(JObject content, IDataMergeContext context, IAllowSync allowSync)
        {
            return Task.CompletedTask;
        }

        public abstract Task AddSyncComponents(JObject content, IDataMergeContext context);

        public virtual Task AllowSyncDetaching(IDataMergeContext context, IAllowSync allowSync)
        {
            return Task.CompletedTask;
        }

        public virtual Task AddSyncComponentsDetaching(IDataMergeContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task AllowDelete(JObject content, IDataDeleteContext context, IAllowSync allowSync)
        {
            return Task.CompletedTask;
        }

        public virtual Task DeleteComponents(JObject content, IDataDeleteContext context)
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
