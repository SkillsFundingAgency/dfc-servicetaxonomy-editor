using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IMergeGraphSyncer
    {
        IMergeNodeCommand MergeNodeCommand { get; }
        IGraphMergeContext? GraphMergeContext { get; }

        Task<IAllowSyncResult> SyncToGraphReplicaSetIfAllowed(
            IGraphReplicaSet graphReplicaSet,
            ContentItem contentItem,
            IContentManager contentManager,
            IGraphMergeContext? parentGraphMergeContext = null);

        /// <summary>
        /// We need to keep the published and preview graph replica sets consistent.
        /// As they contain different data, a sync may fail in one and succeed in t'other,
        /// and when that happens it means the graphs become inconsistent.
        /// So we perform a two phase sync, querying the replica set first to
        /// see if we expect the sync to work.
        /// Only if we expect the sync to work, do we go ahead and sync to both replica sets.
        /// (There could still be transient issues which cause the sync to fail,
        /// but we have the graph validation process to handle those cases.)
        ///
        /// Example single phase scenario demoing issue:
        /// One page has preview version that is only user of a page location.
        /// User deletes that page location and published taxonomy.
        /// Preview graph sync fails (correctly) as location is used by preview page.
        /// Published sync works as no page uses location.
        ///
        /// If any sync components blocks the sync during the check phase,
        /// we cancel the publish of the item into oc's database,
        /// so that OC's DB and the published and preview sets remain consistent.
        ///
        /// Notes:
        /// Neo4j doesn't support distributed write transactions or 2-phase commits
        /// (which would have been useful).
        ///
        /// There is a small window between the check and sync phases, where other users
        /// could have changed the data, and then the db's could get out of sync, but it should mostly work.
        /// </summary>
        Task<IAllowSyncResult> SyncAllowed(
            IGraphReplicaSet graphReplicaSet,
            ContentItem contentItem,
            IContentManager contentManager,
            IGraphMergeContext? parentGraphMergeContext = null);

        Task<IMergeNodeCommand?> SyncToGraphReplicaSet();
    }
}
