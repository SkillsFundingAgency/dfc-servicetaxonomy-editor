using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Configuration;
using DFC.ServiceTaxonomy.Editor.Module.Services;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.Editor.UnitTests.Module.Services
{
    public class NeoGraphDatabaseTests
    {
        [Fact]
        public void NothingTest()
        {
            //todo: SFA.DAS.Testing?? more general equivalent?
            //todo: fluentassertions??
            //todo: autofixture?? AutoFixture, AutoFixture.xUnit2, AutoFakeItEasy
            var optionsMonitor = A.Fake<IOptionsMonitor<Neo4jConfiguration>>();
            var neo4jConfiguration = A.Fake<Neo4jConfiguration>();
            neo4jConfiguration.Endpoint.Uri = "bolt://example.com";
            A.CallTo(() => optionsMonitor.CurrentValue).Returns(neo4jConfiguration);

            var neo4jGraphDatabase = new NeoGraphDatabase(optionsMonitor);

            //await neo4jGraphDatabase.MergeNodeStatement("", new Dictionary<string, object>());
        }
    }
}
