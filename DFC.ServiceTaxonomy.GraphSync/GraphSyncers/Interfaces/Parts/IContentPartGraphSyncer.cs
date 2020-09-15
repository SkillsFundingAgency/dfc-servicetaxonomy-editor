using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts
{
    public interface IContentPartGraphSyncer
    {
        public int Priority { get; }
        string PartName { get; }

        bool CanSync(string contentType, ContentPartDefinition contentPartDefinition);

        Task AllowSync(JObject content, IGraphMergeContext context, IAllowSync allowSync);
        //todo: have new interface for IContainedContentPartGraphSyncer : IContentPartGraphSyncer?????
        Task AddSyncComponents(JObject content, IGraphMergeContext context);

        Task AllowSyncDetaching(IGraphMergeContext context, IAllowSync allowSync);
        Task AddSyncComponentsDetaching(IGraphMergeContext context);

        Task AllowDelete(JObject content, IGraphDeleteContext context, IAllowSync allowSync);
        Task DeleteComponents(JObject content, IGraphDeleteContext context);

        Task MutateOnClone(JObject content, ICloneContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context);

        Task AddRelationship(IDescribeRelationshipsContext context);
    }
}
