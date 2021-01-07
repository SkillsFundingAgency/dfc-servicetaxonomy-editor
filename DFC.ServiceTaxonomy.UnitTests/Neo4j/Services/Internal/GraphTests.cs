#if REPLICA_DISABLING_NET5_ONLY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
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
        internal ICommand[] Commands { get; set; }
        internal ManualResetEventSlim TestFinished { get; set; }
        internal const string GraphName = "Steffi";
        internal const bool DefaultGraph = true;

        public GraphTests()
        {
            NeoEndpoint = A.Fake<INeoEndpoint>();
            Logger = A.Fake<ILogger>();

            Graph = new Graph(NeoEndpoint, GraphName, DefaultGraph, 0, Logger);

            Queries = new[] {A.Fake<IQuery<int>>()};
            Commands = new[] {A.Fake<ICommand>()};

            TestFinished = new ManualResetEventSlim(false);
        }

        [Fact]
        public async Task InFlightCount_RunQueries_CountMatchesInFlightQueries()
        {
            // arrange
            const int inFlightRuns = 10;

            A.CallTo(() => NeoEndpoint.Run(Queries, GraphName, DefaultGraph))
                .ReturnsLazily(() =>
                {
                    TestFinished.Wait();
                    return new List<int>();
                });

            var tasks = Enumerable.Range(0, inFlightRuns).Select(i => Task.Run(() => Graph.Run(Queries)));

            await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(2000));

            // act + assert
            Assert.Equal((ulong)inFlightRuns, Graph.InFlightCount);

            // clean up
            TestFinished.Set();
            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task InFlightCount_RunCommands_CountMatchesInFlightCommands()
        {
            // arrange
            const int inFlightRuns = 10;

            A.CallTo(() => NeoEndpoint.Run(Commands, GraphName, DefaultGraph))
                .ReturnsLazily(() =>
                {
                    TestFinished.Wait();
                    return Task.CompletedTask;
                });

            var tasks = Enumerable.Range(0, inFlightRuns).Select(i => Task.Run(() => Graph.Run(Commands)));

            await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(2000));

            // act + assert
            Assert.Equal((ulong)inFlightRuns, Graph.InFlightCount);

            // clean up
            TestFinished.Set();
            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task InFlightCount_RunQueriesAndCommands_CountMatchesInFlightQueries()
        {
            // arrange
            const int inFlightQueryRuns = 5;
            const int inFlightCommandRuns = 5;

            A.CallTo(() => NeoEndpoint.Run(Queries, GraphName, DefaultGraph))
                .ReturnsLazily(() =>
                {
                    TestFinished.Wait();
                    return new List<int>();
                });

            A.CallTo(() => NeoEndpoint.Run(Commands, GraphName, DefaultGraph))
                .ReturnsLazily(() =>
                {
                    TestFinished.Wait();
                    return Task.CompletedTask;
                });

            var queryTasks = Enumerable.Range(0, inFlightQueryRuns).Select(i => Task.Run(() => Graph.Run(Queries)));
            var commandTasks = Enumerable.Range(0, inFlightCommandRuns).Select(i => Task.Run(() => Graph.Run(Commands)));

            await Task.WhenAny(Task.WhenAll(queryTasks), Task.WhenAll(commandTasks), Task.Delay(2000));

            // act + assert
            Assert.Equal((ulong)inFlightQueryRuns + inFlightCommandRuns, Graph.InFlightCount);

            // clean up
            TestFinished.Set();
            await Task.WhenAll(Task.WhenAll(queryTasks), Task.WhenAll(commandTasks));
        }

        [Fact]
        public void InFlightCount_RunQueriesThrewException_CountIs0()
        {
            A.CallTo(() => NeoEndpoint.Run(Queries, GraphName, DefaultGraph))
                .Throws<Exception>();

            Assert.Equal(0ul, Graph.InFlightCount);
        }

        [Fact]
        public void InFlightCount_RunCommandsThrewException_CountIs0()
        {
            A.CallTo(() => NeoEndpoint.Run(Commands, GraphName, DefaultGraph))
                .Throws<Exception>();

            Assert.Equal(0ul, Graph.InFlightCount);
        }
    }
}
#endif
