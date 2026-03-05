using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts
{
    public interface IContentPartGraphSyncer
    {
        public int Priority { get; }
        string PartName { get; }

        bool CanSync(string contentType, ContentPartDefinition contentPartDefinition);

        Task AllowSync(JsonObject content, IGraphMergeContext context, IAllowSync allowSync);
        //todo: have new interface for IContainedContentPartGraphSyncer : IContentPartGraphSyncer?????
        Task AddSyncComponents(JsonObject content, IGraphMergeContext context);

        Task AllowSyncDetaching(IGraphMergeContext context, IAllowSync allowSync);
        Task AddSyncComponentsDetaching(IGraphMergeContext context);

        Task AllowDelete(JsonObject content, IGraphDeleteContext context, IAllowSync allowSync);
        Task DeleteComponents(JsonObject content, IGraphDeleteContext context);

        Task MutateOnClone(JsonObject content, ICloneContext context);

        Task AddRelationship(JsonObject content, IDescribeRelationshipsContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JsonObject content,
            IValidateAndRepairContext context);
    }
}
