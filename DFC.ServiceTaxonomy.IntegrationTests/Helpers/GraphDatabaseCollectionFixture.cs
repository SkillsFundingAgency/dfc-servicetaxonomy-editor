using System;
using System.IO;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphDatabaseCollectionFixture : IDisposable
    {
        public TestNeoGraphDatabase GraphTestDatabase { get; }

        public GraphDatabaseCollectionFixture()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            Neo4jConfiguration neo4jConfiguration = configuration.GetSection("Neo4j").Get<Neo4jConfiguration>();

            var optionsMonitor = A.Fake<IOptionsMonitor<Neo4jConfiguration>>();
            A.CallTo(() => optionsMonitor.CurrentValue).Returns(neo4jConfiguration);

            GraphTestDatabase = new TestNeoGraphDatabase(optionsMonitor);
        }

        public void Dispose()
        {
            GraphTestDatabase?.Dispose();
        }
    }
}
