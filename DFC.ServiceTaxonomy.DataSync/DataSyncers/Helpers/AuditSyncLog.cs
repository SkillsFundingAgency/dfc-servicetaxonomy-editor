using System;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers
{
    public class AuditSyncLog
    {
        public AuditSyncLog(DateTime lastSynced) => LastSynced = lastSynced;

        public DateTime LastSynced { get; }
    }
}
