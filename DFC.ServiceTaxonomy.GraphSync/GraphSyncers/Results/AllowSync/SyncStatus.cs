namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync
{
    public enum SyncStatus
    {
        /// <summary>
        /// Syncing is not required, because either the ContentItem doesn't have a GraphSyncPart,
        /// or syncing has been temporarily disabled for the content item.
        /// </summary>
        NotRequired,
        /// <summary>
        /// All components have given the go ahead.
        /// </summary>
        Allowed,
        /// <summary>
        /// A component has blocked the sync.
        /// As neo4j doesn't support 2 phase commit, or write transactions across graphs (even in fabric),
        /// we allow components to block a sync before we attempt to sync.
        /// This allows us to keep OC's db, and the published and preview graphs consistent
        /// (even though there is a small window to break this).
        /// </summary>
        Blocked
    }
}
