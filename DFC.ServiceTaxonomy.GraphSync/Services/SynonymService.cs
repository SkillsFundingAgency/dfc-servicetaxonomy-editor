using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class SynonymService : ISynonymService
    {
        private readonly IGraphCluster _neoGraphCluster;
        private readonly IGraphSyncHelper _graphSyncHelper;

        public SynonymService(IGraphCluster neoGraphCluster, IGraphSyncHelper graphSyncHelper)
        {
            _neoGraphCluster = neoGraphCluster ?? throw new ArgumentNullException(nameof(neoGraphCluster));
            _graphSyncHelper = graphSyncHelper ?? throw new ArgumentNullException(nameof(graphSyncHelper));
        }

        //todo: analyser will need to send which replica set it wants the synonyms for
        public async Task<IEnumerable<string>> GetSynonymsAsync(string node)
        {
            //Ensure first character is uppercase for GraphSyncHelper
            node = char.ToUpper(node[0]) + node.Substring(1);

            _graphSyncHelper.ContentType = node;

            var sourceNodeName = await _graphSyncHelper.NodeLabels(node);

            if(node.Equals("Skill", StringComparison.CurrentCultureIgnoreCase))
            {
                return await ExecuteSynonymQueryAsync(new MatchSynonymsQuery(string.Join(":",sourceNodeName), "SkillLabel", "skos__prefLabel", "hasAltLabel", "hasPrefLabel", "hasHiddenLabel"));
            }

            if (node.Equals("Occupation", StringComparison.CurrentCultureIgnoreCase))
            {
                return await ExecuteSynonymQueryAsync(new MatchSynonymsQuery(string.Join(":", sourceNodeName), "OccupationLabel", "skos__prefLabel", "hasAltLabel", "hasPrefLabel", "hasHiddenLabel"));
            }

            return new List<string>();
        }

        private async Task<IEnumerable<string>> ExecuteSynonymQueryAsync(MatchSynonymsQuery query)
        {
            var result = await _neoGraphCluster.Run(GraphReplicaSetNames.Published, query);
            IReadOnlyDictionary<string, object> synonymResults = (IReadOnlyDictionary<string, object>)result.FirstOrDefault().Values["results"];

            var synonymList = ((List<object>)synonymResults.Values.FirstOrDefault()).OfType<string>();

            return synonymList;
        }
    }
}
