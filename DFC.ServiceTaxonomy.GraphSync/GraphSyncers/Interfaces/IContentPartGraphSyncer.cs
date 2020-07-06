using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ValidateAndRepair;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentPartGraphSyncer
    {
        string PartName {get;}

        public bool CanHandle(string contentType, ContentPartDefinition contentPartDefinition)
        {
            return contentPartDefinition.Name == PartName;
        }

        //todo: have new interface for IContainedContentPartGraphSyncer : IContentPartGraphSyncer?????
        Task AddSyncComponents(JObject content, IGraphMergeContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content, ValidateAndRepairContext context);
    }
}
