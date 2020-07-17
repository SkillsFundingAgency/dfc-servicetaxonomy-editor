using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface IValidateAndRepairGraph
    {
        Task<ValidateAndRepairResults> ValidateGraph(params string[] graphReplicaSetNames);
        Task<(bool validated, string failureReason)> ValidateContentItem(
            ContentItem contentItem,
            ContentTypeDefinition contentTypeDefinition,
            IContentItemVersion contentItemVersion);

        string FailureContext(string failureReason, ContentItem contentItem);
    }
}
