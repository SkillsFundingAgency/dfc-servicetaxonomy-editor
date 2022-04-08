using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IValidateAndRepairContext : IGraphOperationContext
    {
        ISubgraph NodeWithRelationships { get; }
        IGraphValidationHelper GraphValidationHelper { get; }
        IDictionary<string, int> ExpectedRelationshipCounts { get; }
        IValidateAndRepairGraph ValidateAndRepairGraph { get; }
    }
}
