using System.IO;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Microsoft.Extensions.Configuration;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphClusterCollectionFixture
    {
        internal Neo4jOptions Neo4jOptions { get; }

        public GraphClusterCollectionFixture()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Neo4jOptions = configuration.GetSection("Neo4j").Get<Neo4jOptions>();

            if (!Neo4jOptions.Endpoints.Any())
                throw new GraphClusterConfigurationErrorException("No endpoints configured.");

            if (!Neo4jOptions.ReplicaSets.Any())
                throw new GraphClusterConfigurationErrorException("No replica sets configured.");
        }
    }
}
