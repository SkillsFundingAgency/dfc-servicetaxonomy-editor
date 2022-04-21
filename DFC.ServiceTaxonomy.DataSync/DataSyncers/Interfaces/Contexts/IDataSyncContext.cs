using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts
{
    public interface IDataSyncContext : IDataSyncOperationContext
    {
        IDataSyncContext? ParentContext { get; }
        IEnumerable<IDataSyncContext> ChildContexts { get; }

        void AddChildContext(IDataSyncContext dataDeleteContext);
    }
}
