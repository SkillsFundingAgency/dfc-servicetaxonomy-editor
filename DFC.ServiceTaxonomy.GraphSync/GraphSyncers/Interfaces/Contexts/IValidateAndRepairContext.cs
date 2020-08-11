using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IValidateAndRepairContext : IGraphOperationContext
    {
        INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; }
        IGraphValidationHelper GraphValidationHelper { get; }
        IDictionary<string, int> ExpectedRelationshipCounts { get; }
        IValidateAndRepairGraph ValidateAndRepairGraph { get; }
    }
}
