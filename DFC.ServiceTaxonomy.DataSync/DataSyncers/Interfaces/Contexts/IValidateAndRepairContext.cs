using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts
{
    public interface IValidateAndRepairContext : IDataSyncOperationContext
    {
        ISubDataSync NodeWithRelationships { get; }
        IDataSyncValidationHelper DataSyncValidationHelper { get; }
        IDictionary<string, int> ExpectedRelationshipCounts { get; }
        IValidateAndRepairData ValidateAndRepairData { get; }
    }
}
