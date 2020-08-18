using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync
{
    // we could have this as part of the context
    // if we did, we'd get the embedded hierarchy for free, with the results in the chained contexts
    public class AllowSyncResult : IAllowSyncResult
    {
        public static IAllowSyncResult NotRequired => new AllowSyncResult {AllowSync = SyncStatus.NotRequired};

        public ConcurrentBag<ISyncBlocker> SyncBlockers { get; set; } = new ConcurrentBag<ISyncBlocker>();
        public SyncStatus AllowSync { get; private set; } = SyncStatus.Allowed;

        public void AddSyncBlocker(ISyncBlocker syncBlocker)
        {
            AllowSync = SyncStatus.Blocked;
            SyncBlockers.Add(syncBlocker);
        }

        public void AddSyncBlockers(IEnumerable<ISyncBlocker> syncBlockers)
        {
            if (!syncBlockers.Any())
                return;

            AllowSync = SyncStatus.Blocked;
            SyncBlockers = new ConcurrentBag<ISyncBlocker>(SyncBlockers.Union(syncBlockers));
        }

        public void AddRelated(IAllowSyncResult allowSyncResult)
        {
            if (allowSyncResult.AllowSync != SyncStatus.Blocked)
                return;

            AllowSync = SyncStatus.Blocked;
            SyncBlockers = new ConcurrentBag<ISyncBlocker>(SyncBlockers.Union(allowSyncResult.SyncBlockers));
        }

        public override string ToString()
        {
            return string.Join(", ", SyncBlockers);
        }
    }
}
