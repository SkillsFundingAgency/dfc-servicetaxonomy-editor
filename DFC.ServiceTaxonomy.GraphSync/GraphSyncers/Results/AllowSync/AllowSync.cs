using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync
{
    // we could have this as part of the context
    // if we did, we'd get the embedded hierarchy for free, with the results in the chained contexts
    public class AllowSync : IAllowSync
    {
        public static IAllowSync NotRequired => new AllowSync {Result = AllowSyncResult.NotRequired};

        public static IAllowSync TestPublishedBlocked => new AllowSync {Result = AllowSyncResult.Blocked,
            SyncBlockers = new ConcurrentBag<ISyncBlocker>
            {
                new SyncBlocker("Test A", "http://localhost:7071/api/execute/page/d3359117-c14d-4ff4-a6d5-357ad9c65e41", "Page"),
                new SyncBlocker("Test B", "http://localhost:7071/api/execute/page/d3359117-c14d-4ff4-a6d5-357ad9c65e41", "Page")
            }};

        public AllowSyncResult Result { get; private set; } = AllowSyncResult.Allowed;
        public ConcurrentBag<ISyncBlocker> SyncBlockers { get; set; } = new ConcurrentBag<ISyncBlocker>();

        public void AddSyncBlocker(ISyncBlocker syncBlocker)
        {
            Result = AllowSyncResult.Blocked;
            SyncBlockers.Add(syncBlocker);
        }

        public void AddSyncBlockers(IEnumerable<ISyncBlocker> syncBlockers)
        {
            if (!syncBlockers.Any())
                return;

            Result = AllowSyncResult.Blocked;
            SyncBlockers = new ConcurrentBag<ISyncBlocker>(SyncBlockers.Union(syncBlockers));
        }

        public void AddRelated(IAllowSync allowSync)
        {
            if (allowSync.Result != AllowSyncResult.Blocked)
                return;

            Result = AllowSyncResult.Blocked;
            SyncBlockers = new ConcurrentBag<ISyncBlocker>(SyncBlockers.Union(allowSync.SyncBlockers));
        }

        public override string ToString()
        {
            return string.Join(", ", SyncBlockers);
        }
    }
}
