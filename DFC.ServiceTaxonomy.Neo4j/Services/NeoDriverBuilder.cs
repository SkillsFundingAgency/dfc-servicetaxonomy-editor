using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Models;
using DFC.ServiceTaxonomy.Neo4j.Models.Interface;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    public class NeoDriverBuilder : INeoDriverBuilder
    {
        private readonly IOptionsMonitor<Neo4jConfiguration> _neo4JConfigurationOptions;
        private readonly ILogger _logger;

        public NeoDriverBuilder(IOptionsMonitor<Neo4jConfiguration> neo4jConfigurationOptions, ILogger logger)
        {
            _neo4JConfigurationOptions = neo4jConfigurationOptions;
            _logger = logger;
        }
        public IEnumerable<INeoDriver> Build()
        {
            //TODO: GroupBy clause shouldn't be needed, but configuration provides duplicates for some reason
            //A driver defined as primary is used for queries/reads
            //Wrap the driver creation process to enable mocking
            return _neo4JConfigurationOptions.CurrentValue.Endpoints.Where(x => x.Enabled).GroupBy(y => y.Uri).Select(z => new NeoDriver(z.FirstOrDefault().Primary ? "Primary" : "Secondary", GraphDatabase.Driver(
                    z.FirstOrDefault().Uri,
                    AuthTokens.Basic(z.FirstOrDefault().Username, z.FirstOrDefault().Password),
                    o => o.WithLogger(_logger)), z.FirstOrDefault().Uri));
        }
    }
}
