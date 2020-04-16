using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Services;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Xunit;
using System.Linq;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Services
{
    public class NeoDriverBuilderTests
    {
        private readonly IOptionsMonitor<Neo4jConfiguration> _optionsMonitor;

        public NeoDriverBuilderTests()
        {
            _optionsMonitor = A.Fake<IOptionsMonitor<Neo4jConfiguration>>();
            var neo4jConfiguration = A.Fake<Neo4jConfiguration>();
            neo4jConfiguration.Endpoints = new List<EndpointConfiguration> { new EndpointConfiguration { Uri = "bolt://example.com", Enabled = true }, new EndpointConfiguration { Uri = "bolt://anotherexample.com", Enabled = true } };
            A.CallTo(() => _optionsMonitor.CurrentValue).Returns(neo4jConfiguration);

        }

        [Fact]
        public void NeoDriverBuilder_WhenBuildCalled_ReturnsDrivers()
        {
            var driverBuilder = new NeoDriverBuilder(_optionsMonitor, A.Fake<ILogger>());

            var result = driverBuilder.Build();

            Assert.Equal(2, result.Count());
        }
    }
}
