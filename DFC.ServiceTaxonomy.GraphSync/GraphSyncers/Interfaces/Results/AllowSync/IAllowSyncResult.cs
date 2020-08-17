using System.Collections.Concurrent;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync
{
    public interface IAllowSyncResult
    {
        SyncStatus AllowSync { get; }

        ConcurrentBag<ISyncBlocker> SyncBlockers { get; set; }

        void AddSyncBlocker(ISyncBlocker syncBlocker);

        void AddSyncBlockers(IEnumerable<ISyncBlocker> syncBlockers);
        // better name?
        void AddRelated(IAllowSyncResult allowSyncResult);
    }
}
