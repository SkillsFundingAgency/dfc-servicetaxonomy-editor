using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentPartGraphSyncer
    {
        string PartName {get;}

        //todo: return userdata or add through context? think can be stored in the part syncer itself
//        Task<(bool AllowSync, object? UserData)> AllowSync(JArray? contentItems, IGraphMergeContext context)
        Task<bool> AllowSync(JArray? contentItems, IGraphMergeContext context) => Task.FromResult(true);

        bool CanSync(string contentType, ContentPartDefinition contentPartDefinition)
        {
            return contentPartDefinition.Name == PartName;
        }

        //todo: have new interface for IContainedContentPartGraphSyncer : IContentPartGraphSyncer?????
        Task AddSyncComponents(JObject content, IGraphMergeContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context);
    }
}
