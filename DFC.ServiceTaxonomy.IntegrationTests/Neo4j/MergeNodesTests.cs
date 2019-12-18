using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Generators;
using DFC.ServiceTaxonomy.Neo4j.Services;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j
{
    public class MergeNodesTests
    {
        private readonly NeoGraphDatabase _neoGraphDatabase;

        public MergeNodesTests()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            Neo4jConfiguration neo4jConfiguration = configuration.GetSection("Neo4j").Get<Neo4jConfiguration>();

            var optionsMonitor = A.Fake<IOptionsMonitor<Neo4jConfiguration>>();
            A.CallTo(() => optionsMonitor.CurrentValue).Returns(neo4jConfiguration);

            _neoGraphDatabase = new NeoGraphDatabase(optionsMonitor);
        }

        [Fact]
        public async Task CreateNodeTest()
        {
            const string nodeLabel = "testNode";
            const string idPropertyName = "testProperty";
            IDictionary<string, object> testProperties = new ReadOnlyDictionary<string, object>(
                new Dictionary<string, object> {{idPropertyName, Guid.NewGuid().ToString()}});

            Query query = QueryGenerator.MergeNodes(nodeLabel, testProperties, idPropertyName);

            //todo: create test NeoGraphDatabase that keeps trans open, then rollbacks after test?
            await _neoGraphDatabase.RunWriteQueries(query);
        }
    }
}
