using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.Indexes;
using DFC.ServiceTaxonomy.DataSync.Services.Interface;
using OrchardCore.ContentManagement;
using YesSql;

namespace DFC.ServiceTaxonomy.DataSync.Services
{
    //todo: better name
    public class NodeContentItemLookup : INodeContentItemLookup
    {
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly ISuperpositionContentItemVersion _superpositionContentItemVersion;
        private readonly IEscoContentItemVersion _escoContentItemVersion;
        private readonly ISession _session;

        public NodeContentItemLookup(
            IContentItemVersionFactory contentItemVersionFactory,
            ISyncNameProvider syncNameProvider,
            ISuperpositionContentItemVersion superpositionContentItemVersion,
            IEscoContentItemVersion escoContentItemVersion,
            ISession session)
        {
            _contentItemVersionFactory = contentItemVersionFactory;
            _syncNameProvider = syncNameProvider;
            _superpositionContentItemVersion = superpositionContentItemVersion;
            _escoContentItemVersion = escoContentItemVersion;
            _session = session;
        }

        //todo: nodeId should be object
        public async Task<string?> GetContentItemId(string nodeId, string graphReplicaSetName)
        {
            IContentItemVersion contentItemVersion = _contentItemVersionFactory.Get(graphReplicaSetName);

            // what we should be doing, but we'd have to pass along the node labels to get the content type
            //ISyncNameProvider syncNameProvider = _serviceProvider.GetSyncNameProvider(pickedContentType);

            //string graphSyncNodeId = syncNameProvider.IdPropertyValueFromNodeValue(nodeId, contentItemVersion);

            string graphSyncNodeId = _syncNameProvider.ConvertIdPropertyValue(nodeId,
                _superpositionContentItemVersion,
                contentItemVersion, _escoContentItemVersion);

            var contentItems = await _session
                .Query<ContentItem, GraphSyncPartIndex>(x => x.NodeId == graphSyncNodeId)
                .ListAsync();

            return contentItems?.FirstOrDefault()?.ContentItemId;
        }
    }
}
