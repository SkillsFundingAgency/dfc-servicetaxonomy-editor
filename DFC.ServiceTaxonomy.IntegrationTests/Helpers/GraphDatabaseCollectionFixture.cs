using System;
using System.IO;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using FakeItEasy;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public class GraphDatabaseCollectionFixture : IDisposable
    {
        public TestNeoGraphDatabase GraphTestDatabase { get; }
        public CompareLogic CompareLogic { get; }

        public GraphDatabaseCollectionFixture()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            Neo4jOptions neo4jOptions = configuration.GetSection("Neo4j").Get<Neo4jOptions>();

            var optionsMonitor = A.Fake<IOptionsMonitor<Neo4jOptions>>();
            A.CallTo(() => optionsMonitor.CurrentValue).Returns(neo4jOptions);

            GraphTestDatabase = new TestNeoGraphDatabase(optionsMonitor);

            CompareLogic = new CompareLogic();
            CompareLogic.Config.IgnoreProperty<ExpectedNode>(n => n.Id);
            CompareLogic.Config.IgnoreProperty<ExpectedRelationship>(r => r.Id);
            CompareLogic.Config.IgnoreObjectTypes = true;
            CompareLogic.Config.SkipInvalidIndexers = true;
            CompareLogic.Config.MaxDifferences = 10;
        }

        public void Dispose()
        {
            GraphTestDatabase?.Dispose();
        }
    }
}
