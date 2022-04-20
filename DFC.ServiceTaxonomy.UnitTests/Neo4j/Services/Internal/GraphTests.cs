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
        internal DataSync DataSync { get; set; }
        internal INeoEndpoint NeoEndpoint { get; set; }
        internal ILogger Logger { get; set; }
        internal IQuery<int>[] Queries { get; set; }
        internal ICommand[] Commands { get; set; }
        internal ManualResetEventSlim TestFinished { get; set; }
        internal const string DataSyncName = "Steffi";
        internal const bool DefaultDataSync = true;

        public GraphTests()
        {
            NeoEndpoint = A.Fake<INeoEndpoint>();
            Logger = A.Fake<ILogger>();

            DataSync = new DataSync(NeoEndpoint, DataSyncName, DefaultDataSync, 0, Logger);

            Queries = new[] {A.Fake<IQuery<int>>()};
            Commands = new[] {A.Fake<ICommand>()};

            TestFinished = new ManualResetEventSlim(false);
        }

        [Fact]
        public async Task InFlightCount_RunQueries_CountMatchesInFlightQueries()
        {
            // arrange
            const int inFlightRuns = 10;

            A.CallTo(() => NeoEndpoint.Run(Queries, DataSyncName, DefaultDataSync))
                .ReturnsLazily(() =>
                {
                    TestFinished.Wait();
                    return new List<int>();
                });

            var tasks = Enumerable.Range(0, inFlightRuns).Select(i => Task.Run(() => DataSync.Run(Queries)));

            await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(2000));

            // act + assert
            Assert.Equal((ulong)inFlightRuns, DataSync.InFlightCount);

            // clean up
            TestFinished.Set();
            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task InFlightCount_RunCommands_CountMatchesInFlightCommands()
        {
            // arrange
            const int inFlightRuns = 10;

            A.CallTo(() => NeoEndpoint.Run(Commands, DataSyncName, DefaultDataSync))
                .ReturnsLazily(() =>
                {
                    TestFinished.Wait();
                    return Task.CompletedTask;
                });

            var tasks = Enumerable.Range(0, inFlightRuns).Select(i => Task.Run(() => DataSync.Run(Commands)));

            await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(2000));

            // act + assert
            Assert.Equal((ulong)inFlightRuns, DataSync.InFlightCount);

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

            A.CallTo(() => NeoEndpoint.Run(Queries, DataSyncName, DefaultDataSync))
                .ReturnsLazily(() =>
                {
                    TestFinished.Wait();
                    return new List<int>();
                });

            A.CallTo(() => NeoEndpoint.Run(Commands, DataSyncName, DefaultDataSync))
                .ReturnsLazily(() =>
                {
                    TestFinished.Wait();
                    return Task.CompletedTask;
                });

            var queryTasks = Enumerable.Range(0, inFlightQueryRuns).Select(i => Task.Run(() => DataSync.Run(Queries)));
            var commandTasks = Enumerable.Range(0, inFlightCommandRuns).Select(i => Task.Run(() => DataSync.Run(Commands)));

            await Task.WhenAny(Task.WhenAll(queryTasks), Task.WhenAll(commandTasks), Task.Delay(2000));

            // act + assert
            Assert.Equal((ulong)inFlightQueryRuns + inFlightCommandRuns, DataSync.InFlightCount);

            // clean up
            TestFinished.Set();
            await Task.WhenAll(Task.WhenAll(queryTasks), Task.WhenAll(commandTasks));
        }

        [Fact]
        public void InFlightCount_RunQueriesThrewException_CountIs0()
        {
            A.CallTo(() => NeoEndpoint.Run(Queries, DataSyncName, DefaultDataSync))
                .Throws<Exception>();

            Assert.Equal(0ul, DataSync.InFlightCount);
        }

        [Fact]
        public void InFlightCount_RunCommandsThrewException_CountIs0()
        {
            A.CallTo(() => NeoEndpoint.Run(Commands, DataSyncName, DefaultDataSync))
                .Throws<Exception>();

            Assert.Equal(0ul, DataSync.InFlightCount);
        }
    }
}
#endif
