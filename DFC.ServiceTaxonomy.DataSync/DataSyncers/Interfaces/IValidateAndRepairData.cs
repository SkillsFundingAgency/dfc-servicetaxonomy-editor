using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.ValidateAndRepair;
using DFC.ServiceTaxonomy.DataSync.Enums;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces
{
    public interface IValidateAndRepairData
    {
        Task<IValidateAndRepairResults> ValidateData(
            ValidationScope validationScope,
            params string[] dataSyncReplicaSetNames);

        Task<(bool validated, string failureReason)> ValidateContentItem(
            ContentItem contentItem,
            ContentTypeDefinition contentTypeDefinition,
            IContentItemVersion contentItemVersion);

        string FailureContext(string failureReason, ContentItem contentItem);
    }
}
