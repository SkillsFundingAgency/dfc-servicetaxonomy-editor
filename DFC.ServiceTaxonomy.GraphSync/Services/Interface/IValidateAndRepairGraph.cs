﻿using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface IValidateAndRepairGraph
    {
        Task<ValidateAndRepairResults> ValidateGraph(params string[] graphReplicaSetNames);
        Task<(bool validated, string failureReason)> ValidateContentItem(
            ContentItem contentItem,
            ContentTypeDefinition contentTypeDefinition);
    }
}
