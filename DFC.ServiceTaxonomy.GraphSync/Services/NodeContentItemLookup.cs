using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Indexes;
using OrchardCore.ContentManagement;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    //todo: better name
    public class NodeContentItemLookup
    {
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly ISuperpositionContentItemVersion _superpositionContentItemVersion;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly ISession _session;

        public NodeContentItemLookup(
            IContentItemVersionFactory contentItemVersionFactory,
            ISuperpositionContentItemVersion superpositionContentItemVersion,
            ISyncNameProvider syncNameProvider,
            ISession session)
        {
            _contentItemVersionFactory = contentItemVersionFactory;
            _superpositionContentItemVersion = superpositionContentItemVersion;
            _syncNameProvider = syncNameProvider;
            _session = session;
        }

        //todo: nodeId should be object
        public async Task<string?> GetContentItemId(string nodeId, string graphReplicaSetName)
        {
            IContentItemVersion contentItemVersion = _contentItemVersionFactory.Get(graphReplicaSetName);

            string graphSyncNodeId = _syncNameProvider.IdPropertyValueFromNodeValue(nodeId, contentItemVersion, _superpositionContentItemVersion);

            var contentItems = await _session.Query<ContentItem, GraphSyncPartIndex>(
                x => x.NodeId == graphSyncNodeId)
                .ListAsync();

            return contentItems?.FirstOrDefault()?.ContentItemId;
        }
    }
}
