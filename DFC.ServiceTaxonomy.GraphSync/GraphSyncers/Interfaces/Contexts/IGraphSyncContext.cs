using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphSyncContext : IGraphOperationContext
    {
        IGraphSyncContext? ParentContext { get; }
        IEnumerable<IGraphSyncContext> ChildContexts { get; }

        void AddChildContext(IGraphSyncContext graphDeleteContext);
    }
}
