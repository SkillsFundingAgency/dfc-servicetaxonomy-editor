using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts
{
    public interface IContentPartGraphSyncer
    {
        int Priority { get => 0; }
        string PartName { get; }

        bool CanSync(string contentType, ContentPartDefinition contentPartDefinition)
        {
            return contentPartDefinition.Name == PartName;
        }

        Task AllowSync(JObject content, IGraphMergeContext context, IAllowSyncResult allowSyncResult) =>
            Task.CompletedTask;

        //todo: have new interface for IContainedContentPartGraphSyncer : IContentPartGraphSyncer?????
        Task AddSyncComponents(JObject content, IGraphMergeContext context);

        Task AllowDelete(JObject content, IGraphDeleteContext context, IAllowSyncResult allowSyncResult) =>
            Task.CompletedTask;

        Task DeleteComponents(JObject content, IGraphDeleteContext context) =>
            Task.CompletedTask;

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context);
    }
}
