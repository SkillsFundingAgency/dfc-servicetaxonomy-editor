using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.ValidateAndRepair;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IValidateAndRepairGraph
    {
        Task<IValidateAndRepairResults> ValidateGraph(
            ValidationScope validationScope,
            params string[] graphReplicaSetNames);

        Task<(bool validated, string failureReason)> ValidateContentItem(
            ContentItem contentItem,
            ContentTypeDefinition contentTypeDefinition,
            IContentItemVersion contentItemVersion);

        string FailureContext(string failureReason, ContentItem contentItem);
    }
}
