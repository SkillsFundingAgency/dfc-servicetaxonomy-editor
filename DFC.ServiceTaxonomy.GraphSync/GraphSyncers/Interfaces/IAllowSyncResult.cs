using System.Collections.Concurrent;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
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
