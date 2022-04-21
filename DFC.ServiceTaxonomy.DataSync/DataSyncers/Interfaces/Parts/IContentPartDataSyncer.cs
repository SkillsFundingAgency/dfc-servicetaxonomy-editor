using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Parts
{
    public interface IContentPartDataSyncer
    {
        public int Priority { get; }
        string PartName { get; }

        bool CanSync(string contentType, ContentPartDefinition contentPartDefinition);

        Task AllowSync(JObject content, IDataMergeContext context, IAllowSync allowSync);
        //todo: have new interface for IContainedContentPartDataSyncer : IContentPartDataSyncer?????
        Task AddSyncComponents(JObject content, IDataMergeContext context);

        Task AllowSyncDetaching(IDataMergeContext context, IAllowSync allowSync);
        Task AddSyncComponentsDetaching(IDataMergeContext context);

        Task AllowDelete(JObject content, IDataDeleteContext context, IAllowSync allowSync);
        Task DeleteComponents(JObject content, IDataDeleteContext context);

        Task MutateOnClone(JObject content, ICloneContext context);

        Task AddRelationship(JObject content, IDescribeRelationshipsContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context);
    }
}
