using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
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

        Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            IContentManager contentManager,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            IValidateAndRepairGraph validateAndRepairGraph);
    }
}
