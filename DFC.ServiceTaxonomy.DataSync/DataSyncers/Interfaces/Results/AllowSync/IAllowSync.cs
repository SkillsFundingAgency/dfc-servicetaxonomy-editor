using System.Collections.Concurrent;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.AllowSync;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync
{
    public interface IAllowSync
    {
        AllowSyncResult Result { get; }

        ConcurrentBag<ISyncBlocker> SyncBlockers { get; set; }

        void AddSyncBlocker(ISyncBlocker syncBlocker);

        void AddSyncBlockers(IEnumerable<ISyncBlocker> syncBlockers);
        // better name?
        void AddRelated(IAllowSync allowSync);
    }
}
