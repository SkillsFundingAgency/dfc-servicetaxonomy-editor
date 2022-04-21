using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.Queries;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Services.Interface;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb
{
    public class CosmosDbSynonymService : ISynonymService
    {
        private readonly IDataSyncCluster _neoDataSyncCluster;
        private readonly ISyncNameProvider _syncNameProvider;

        public CosmosDbSynonymService(IDataSyncCluster neoDataSyncCluster, ISyncNameProvider syncNameProvider)
        {
            _neoDataSyncCluster = neoDataSyncCluster ?? throw new ArgumentNullException(nameof(neoDataSyncCluster));
            _syncNameProvider = syncNameProvider ?? throw new ArgumentNullException(nameof(syncNameProvider));
        }

        //todo: analyser will need to send which replica set it wants the synonyms for
        public async Task<IEnumerable<string>> GetSynonymsAsync(string node)
        {
            // ensure first character is uppercase for SyncNameProvider
            node = char.ToUpper(node[0]) + node.Substring(1);

            _syncNameProvider.ContentType = node;

            var sourceNodeName = await _syncNameProvider.NodeLabels(node);

            if (node.Equals("Skill", StringComparison.CurrentCultureIgnoreCase))
            {
                return await ExecuteSynonymQueryAsync(new CosmosDbMatchSynonymsQuery(string.Join(":",sourceNodeName), "SkillLabel", "skos__prefLabel", "hasAltLabel", "hasPrefLabel", "hasHiddenLabel"));
            }

            if (node.Equals("Occupation", StringComparison.CurrentCultureIgnoreCase))
            {
                return await ExecuteSynonymQueryAsync(new CosmosDbMatchSynonymsQuery(string.Join(":", sourceNodeName), "OccupationLabel", "skos__prefLabel", "hasAltLabel", "hasPrefLabel", "hasHiddenLabel"));
            }

            return new List<string>();
        }

        private async Task<IEnumerable<string>> ExecuteSynonymQueryAsync(CosmosDbMatchSynonymsQuery query)
        {
            List<IRecord> result = await _neoDataSyncCluster.Run(DataSyncReplicaSetNames.Published, query);
            //todo: (at least some of) this code belongs in the queries ProcessRecord()
            //todo: revisit null handling
            IReadOnlyDictionary<string, object>? synonymResults = (IReadOnlyDictionary<string, object>?)result.FirstOrDefault()?.Values["results"];

            return ((List<object>?)synonymResults?.Values.FirstOrDefault())?.OfType<string>() ?? Enumerable.Empty<string>();
        }
    }
}
