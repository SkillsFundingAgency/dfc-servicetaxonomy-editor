using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts
{
    public class ValidateAndRepairContext : DataSyncOperationContext, IValidateAndRepairContext
    {
        public ISubDataSync NodeWithRelationships { get; }
        public IDataSyncValidationHelper DataSyncValidationHelper { get; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; }
        public IValidateAndRepairData ValidateAndRepairData { get; }

        public ValidateAndRepairContext(
            ContentItem contentItem,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            ISubDataSync nodeWithRelationships,
            ISyncNameProvider syncNameProvider,
            IDataSyncValidationHelper dataSyncValidationHelper,
            IValidateAndRepairData validateAndRepairData,
            ILogger logger)
            : base(contentItem, syncNameProvider, contentManager, contentItemVersion, logger)
        {
            ContentItemVersion = contentItemVersion;
            NodeWithRelationships = nodeWithRelationships;
            DataSyncValidationHelper = dataSyncValidationHelper;
            ValidateAndRepairData = validateAndRepairData;

            ExpectedRelationshipCounts = new Dictionary<string, int>();
        }
    }
}
