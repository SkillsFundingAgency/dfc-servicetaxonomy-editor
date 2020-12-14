using System;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Services.Internal
{
    public class GraphTests
    {
        internal Graph Graph { get; set; }
        internal INeoEndpoint NeoEndpoint { get; set; }
        internal ILogger Logger { get; set; }
        internal IQuery<int>[] Queries { get; set; }
        internal const string GraphName = "Steffi";
        internal const bool DefaultGraph = true;

        public GraphTests()
        {
            NeoEndpoint = A.Fake<INeoEndpoint>();
            Logger = A.Fake<ILogger>();

            Graph = new Graph(NeoEndpoint, GraphName, DefaultGraph, 0, Logger);

            Queries = new[] {A.Fake<IQuery<int>>()};
        }

        [Fact]
        public void InFlightCount_RunThrewException_CountIs0()
        {
            A.CallTo(() => NeoEndpoint.Run(Queries, GraphName, DefaultGraph))
                .Throws<Exception>();

            Assert.Equal(0ul, Graph.InFlightCount);
        }
    }
}
