using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Log;
using DFC.ServiceTaxonomy.Neo4j.Models;
using DFC.ServiceTaxonomy.Neo4j.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Services
{
    public class NeoGraphDatabaseTests
    {
        //todo: autofixture?? AutoFixture, AutoFixture.xUnit2, AutoFakeItEasy. auto on ctor possible?

        private readonly NeoGraphDatabase _neoGraphDatabase;

        public NeoGraphDatabaseTests()
        {
            var optionsMonitor = A.Fake<IOptionsMonitor<Neo4jConfiguration>>();
            var neo4jConfiguration = A.Fake<Neo4jConfiguration>();
            neo4jConfiguration.Endpoints = new System.Collections.Generic.List<EndpointConfiguration> { new EndpointConfiguration { Uri = "bolt://example.com" } };
            A.CallTo(() => optionsMonitor.CurrentValue).Returns(neo4jConfiguration);

            var neoDriverBuilder = A.Fake<INeoDriverBuilder>();
            A.CallTo(() => neoDriverBuilder.Build()).Returns(new List<NeoDriver>());
            //todo: we might want to actually log
            var logger = new NeoLogger(new NullLogger<NeoLogger>());

            _neoGraphDatabase = new NeoGraphDatabase(neoDriverBuilder, logger, new NullLogger<NeoGraphDatabase>());
        }
    }
}
