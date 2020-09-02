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

        Task AllowSync(JObject content, IGraphMergeContext context, IAllowSyncResult allowSyncResult);
        //todo: have new interface for IContainedContentPartGraphSyncer : IContentPartGraphSyncer?????
        Task AddSyncComponents(JObject content, IGraphMergeContext context);

        //todo: when resyncing following a part detachment, we can't disallow sync, even if
        // e.g. the sync will fail because an embedded item's in use
        // can we remove AllowSyncDetaching? will still have to go through the 2 phase, but perhaps not
        // call on the parts/fields
        // how do we handle embedded items in use? just detach delete the buggers i guess
        // so removing any incoming relationships
        Task AllowSyncDetaching(IGraphMergeContext context, IAllowSyncResult allowSyncResult);
        Task AddSyncComponentsDetaching(IGraphMergeContext context);

        Task AllowDelete(JObject content, IGraphDeleteContext context, IAllowSyncResult allowSyncResult);
        Task DeleteComponents(JObject content, IGraphDeleteContext context);

        Task MutateOnClone(JObject content, ICloneContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context);
    }
}
