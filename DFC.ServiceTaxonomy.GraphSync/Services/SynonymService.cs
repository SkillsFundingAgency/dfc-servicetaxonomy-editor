using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class SynonymService : ISynonymService
    {
        private readonly IGraphDatabase _neoGraphDatabase;
        private readonly IGraphSyncHelper _graphSyncHelper;

        public SynonymService(IGraphDatabase neoGraphDatabase, IGraphSyncHelper graphSyncHelper)
        {
            _neoGraphDatabase = neoGraphDatabase ?? throw new ArgumentNullException(nameof(neoGraphDatabase));
            _graphSyncHelper = graphSyncHelper ?? throw new ArgumentNullException(nameof(graphSyncHelper));
        }

        public async Task<IEnumerable<string>> GetSynonymsAsync(string node)
        {
            //Ensure first character is uppercase for GraphSyncHelper
            node = char.ToUpper(node[0]) + node.Substring(1);

            _graphSyncHelper.ContentType = node;

            var sourceNodeName = await _graphSyncHelper.NodeLabels(node);

            if(node.Equals("Skill", StringComparison.CurrentCultureIgnoreCase))
            {
                return await ExecuteSynonymQueryAsync(new MatchSynonymsQuery(sourceNodeName.FirstOrDefault(), "ncs__SkillLabel", "skos__prefLabel", "ncs__hasAltLabel", "ncs__hasPrefLabel", "ncs__hasHiddenLabel"));
            }

            if (node.Equals("Occupation", StringComparison.CurrentCultureIgnoreCase))
            {
                return await ExecuteSynonymQueryAsync(new MatchSynonymsQuery(sourceNodeName.FirstOrDefault(), "ncs__OccupationLabel", "skos__prefLabel", "ncs__hasAltLabel", "ncs__hasPrefLabel", "ncs__hasHiddenLabel"));
            }

            return new List<string>();
        }

        private async Task<IEnumerable<string>> ExecuteSynonymQueryAsync(MatchSynonymsQuery query)
        {
            var result = await _neoGraphDatabase.Run(query);

            IReadOnlyDictionary<string, object> synonymResults = (IReadOnlyDictionary<string, object>)result.FirstOrDefault().Values["results"];

            var synonymList = ((List<object>)synonymResults.Values.FirstOrDefault()).OfType<string>();

            return synonymList;
        }
    }
}
