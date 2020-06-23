using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
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

        //todo: change content to JObject
        Task AddSyncComponents(JObject content,
            ContentItem contentItem,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint);
    }
}
