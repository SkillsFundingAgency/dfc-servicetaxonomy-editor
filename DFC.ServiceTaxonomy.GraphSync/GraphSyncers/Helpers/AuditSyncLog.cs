using System;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class AuditSyncLog
    {
        public AuditSyncLog(DateTime lastSynced) => LastSynced = lastSynced;

        public DateTime LastSynced { get; }
    }
}
