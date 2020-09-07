using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IValidateAndRepairContext : IGraphOperationContext
    {
        INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; }
        public INodeWithIncomingRelationships NodeWithIncomingRelationships { get; }
        IGraphValidationHelper GraphValidationHelper { get; }
        IDictionary<string, int> ExpectedRelationshipCounts { get; }
        IValidateAndRepairGraph ValidateAndRepairGraph { get; }
    }
}
