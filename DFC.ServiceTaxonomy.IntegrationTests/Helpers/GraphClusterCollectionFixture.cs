using System.IO;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphClusterCollectionFixture //: IDisposable
    {
        internal Neo4jOptions Neo4jOptions { get; }
        internal ILogger<NeoEndpoint> NLogLogger { get; }
        // public CompareLogic CompareLogic { get; }

        public GraphClusterCollectionFixture()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            Neo4jOptions = configuration.GetSection("Neo4j").Get<Neo4jOptions>();

            NLogLogger = A.Fake<ILogger<NeoEndpoint>>();

            if (!Neo4jOptions.Endpoints.Any())
                throw new GraphClusterConfigurationErrorException("No endpoints configured.");

            if (!Neo4jOptions.ReplicaSets.Any())
                throw new GraphClusterConfigurationErrorException("No replica sets configured.");

            // CompareLogic = new CompareLogic();
            // CompareLogic.Config.IgnoreProperty<ExpectedNode>(n => n.Id);
            // CompareLogic.Config.IgnoreProperty<ExpectedRelationship>(r => r.Id);
            // CompareLogic.Config.IgnoreObjectTypes = true;
            // CompareLogic.Config.SkipInvalidIndexers = true;
            // CompareLogic.Config.MaxDifferences = 10;
        }

        // public void Dispose()
        // {
        //     GraphCluster?.Dispose();
        // }
    }
}
