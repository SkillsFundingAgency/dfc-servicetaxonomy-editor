using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Generators;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using Xunit;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j
{
    public class GraphDatabaseFixture : IDisposable
    {
        public TestNeoGraphDatabase GraphTestDatabase { get; }

        public GraphDatabaseFixture()
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

    [CollectionDefinition("Graph Database Integration")]
    public class GraphDatabaseIntegrationCollection : ICollectionFixture<GraphDatabaseFixture>
    {
    }

    //base class?
    [Collection("Graph Database Integration")]
    public class MergeNodesTests : IAsyncLifetime, IDisposable
    {
        private GraphDatabaseFixture _graphDatabaseFixture;
        private IGraphDatabaseTestRun? _graphDatabaseTestRun;

        public MergeNodesTests(GraphDatabaseFixture graphDatabaseFixture)
        {
            _graphDatabaseFixture = graphDatabaseFixture;
        }

        public async Task InitializeAsync()
        {
            _graphDatabaseTestRun = await _graphDatabaseFixture.GraphTestDatabase.StartTestRun();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public void Dispose() => _graphDatabaseTestRun?.Dispose();

        [Fact]
        public async Task CreateNode_NoExistingNode_Test()
        {
            const string nodeLabel = "testNode";
            const string idPropertyName = "testProperty";
            string idPropertyValue = Guid.NewGuid().ToString();

            IDictionary<string, object> testProperties = new ReadOnlyDictionary<string, object>(
                new Dictionary<string, object> {{idPropertyName, idPropertyValue}});

            Query query = QueryGenerator.MergeNodes(nodeLabel, testProperties, idPropertyName);

            //todo: create test NeoGraphDatabase that keeps trans open, then rollbacks after test?
            await _graphDatabaseTestRun!.RunWriteQueries(query);

            //todo: we could convert to INode here, but if conversion fails, we won't get an assert failing
            List<IRecord> actualRecords = await _graphDatabaseTestRun.RunReadQuery(
                new Query($"match (n:{nodeLabel}) return n"),
                r => r);

            // note: Records on a result cannot be accessed if the session or transaction where the result is created has been closed. (https://github.com/neo4j/neo4j-dotnet-driver)
            // Any query results obtained within a transaction function should be consumed within that function. Transaction functions can return values but these should be derived values rather than raw results. (https://neo4j.com/docs/driver-manual/1.7/sessions-transactions/#driver-transactions)
            //todo: ^^ should probably not ignore this!
            //todo: use reactive session?

            // no decent xUnit support for this sort of thing. use fluentassertions, comparenetobjecs, other?
            //todo: when this fails, it returns a disgusting assert failure message
            Assert.Collection(actualRecords, record =>
            {
                Assert.Collection(record.Values,
                    r =>
                    {
                        //todo: will need a helper for this

                        // meta assert: check we have the correct variable
                        Assert.Equal("n", r.Key);

                        INode node = r.Value.As<INode>();
                        Assert.NotNull(node);

                        Assert.Collection(node.Labels, l =>
                        {
                            Assert.Equal(nodeLabel, l);
                        });
                        Assert.Collection(node.Properties, p =>
                        {
                            Assert.Equal(idPropertyName, p.Key);
                            Assert.Equal(idPropertyValue, p.Value);
                        });
                    });
            });
        }

        // public async Task ReplaceNodeTest()
    }
}
