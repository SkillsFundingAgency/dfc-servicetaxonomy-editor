using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class SynonymService : ISynonymService
    {
        private readonly IGraphDatabase _neoGraphDatabase;

        public SynonymService(IGraphDatabase neoGraphDatabase)
        {
            _neoGraphDatabase = neoGraphDatabase ?? throw new ArgumentNullException(nameof(neoGraphDatabase));
        }

        public IEnumerable<string> GetSynonyms()
        {
            var query = new MatchSynonymsQuery("esco__Occupation", "ncs__OccupationLabel", "skos__prefLabel", "ncs__hasAltLabel", "ncs__hasPrefLabel", "ncs__hasHiddenLabel");
            var result = _neoGraphDatabase.Run(query).GetAwaiter().GetResult();

            IReadOnlyDictionary<string, object> synonymResults = (IReadOnlyDictionary<string, object>)result.FirstOrDefault().Values["results"];

            var synonymList = ((List<object>)synonymResults.Values.FirstOrDefault()).OfType<string>();

            return synonymList;
        }
    }
}
