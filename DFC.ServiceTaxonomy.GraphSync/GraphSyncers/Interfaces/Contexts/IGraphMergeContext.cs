using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphMergeContext : IGraphSyncContext
    {
        //todo: can we use c#9 covariant returns in interfaces?
        new IGraphMergeContext? ParentContext { get; }
        new IEnumerable<IGraphMergeContext> ChildContexts { get; }

        IMergeGraphSyncer MergeGraphSyncer { get; }

        IGraphReplicaSet GraphReplicaSet { get; }
        IMergeNodeCommand MergeNodeCommand { get; }
        IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; }
        List<ICommand> ExtraCommands { get; }
    }
}
