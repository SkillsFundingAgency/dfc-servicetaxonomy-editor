using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbSynonymService : ISynonymService
    {
        private readonly IGraphCluster _neoGraphCluster;
        private readonly ISyncNameProvider _syncNameProvider;

        public CosmosDbSynonymService(IGraphCluster neoGraphCluster, ISyncNameProvider syncNameProvider)
        {
            _neoGraphCluster = neoGraphCluster ?? throw new ArgumentNullException(nameof(neoGraphCluster));
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
            List<IRecord> result = await _neoGraphCluster.Run(GraphReplicaSetNames.Published, query);
            //todo: (at least some of) this code belongs in the queries ProcessRecord()
            //todo: revisit null handling
            IReadOnlyDictionary<string, object>? synonymResults = (IReadOnlyDictionary<string, object>?)result.FirstOrDefault()?.Values["results"];

            return ((List<object>?)synonymResults?.Values.FirstOrDefault())?.OfType<string>() ?? Enumerable.Empty<string>();
        }
    }
}
