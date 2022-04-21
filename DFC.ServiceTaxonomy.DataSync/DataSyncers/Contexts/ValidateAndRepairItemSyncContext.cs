using System;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts
{
    public class ValidateAndRepairItemSyncContext : ValidateAndRepairContext, IValidateAndRepairItemSyncContext
    {
        public ContentTypeDefinition ContentTypeDefinition { get; }
        public object NodeId { get; }

        public ValidateAndRepairItemSyncContext(
            ContentItem contentItem,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            ISubDataSync nodeWithRelationships,
            ISyncNameProvider syncNameProvider,
            IDataSyncValidationHelper dataSyncValidationHelper,
            IValidateAndRepairData validateAndRepairData,
            ContentTypeDefinition contentTypeDefinition,
            object nodeId,
            IServiceProvider serviceProvider)

            : base(contentItem, contentManager, contentItemVersion, nodeWithRelationships,
                syncNameProvider, dataSyncValidationHelper, validateAndRepairData,
                serviceProvider.GetRequiredService<ILogger<ValidateAndRepairItemSyncContext>>())
        {
            ContentTypeDefinition = contentTypeDefinition;
            NodeId = nodeId;
        }
    }
}
