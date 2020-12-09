﻿using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.IntegrationTests.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.IntegrationTests.Neo4j.Services.Internal
{
    [Collection("GraphCluster Integration")]
    public class GraphReplicaSetLowLevel_DisableTests : GraphClusterIntegrationTest
    {
        public IQuery<int> Query { get; }

        public GraphReplicaSetLowLevel_DisableTests(
            GraphClusterCollectionFixture graphClusterCollectionFixture,
            ITestOutputHelper testOutputHelper)
            : base(graphClusterCollectionFixture, testOutputHelper)
        {
            Query = A.Fake<IQuery<int>>();

            // not sure why this doesn't work here?
            // we could get a distinct set of endpoints from the cluster

            //foreach (var endpoint in Endpoints)
            //{
            //    //A.CallTo(() => endpoint.Run(A<IQuery<int>[]>._, A<string>._, A<bool>._))
            //    //    .Invokes(() =>
            //    //    {
            //    //        TestOutputHelper.WriteLine($"Run started on thread #{Thread.CurrentThread.ManagedThreadId}.");
            //    //        Thread.Sleep(100);
            //    //        TestOutputHelper.WriteLine($"Run finished on thread #{Thread.CurrentThread.ManagedThreadId}.");
            //    //    })
            //    //    .Returns(new List<int> { 69 });
            //}
        }

        // manual test (todo: partly automate)
        // checks: (might be a few stragglers, due to delay before trace lines are written)
        //todo: check only enabled replica used after disable() has returned - could be automated?
        //check any query run before disable() has returned is finished first
        [Fact]
        public void DisableWaitsForInFlightQueriesTest()
        {
            const int replicaInstance = 0;

            var replicaSet = GraphClusterLowLevel.GetGraphReplicaSetLowLevel("published");

            Parallel.For(0, 100, async (i, state) =>
            {
                TestOutputHelper.WriteLine($"Iteration {i} on thread #{Thread.CurrentThread.ManagedThreadId}");

                if (i == 5)
                {
                    TestOutputHelper.WriteLine($"Disabling replica #{replicaInstance}. Iteration #{i}. Thread #{Thread.CurrentThread.ManagedThreadId}");
                    TestOutputHelper.WriteLine("----------------------------->>>-------------------------------");
                    replicaSet.Disable(replicaInstance);
                    TestOutputHelper.WriteLine("-----------------------------<<<-------------------------------");
                    TestOutputHelper.WriteLine($"Disabled replica #{replicaInstance}. Iteration #{i}. Thread #{Thread.CurrentThread.ManagedThreadId}");
                }
                else
                {
                    await replicaSet.Run(Query);
                }
            });

            Trace(replicaSet);

            Thread.Sleep(2000);
            // what can we check?, we can't assume all Run()'s before the disabled were started
            // and we can't assume Run()'s from later iteration's weren't started
            // int enabledInstanceCount = replicaSet.EnabledInstanceCount;
            // Assert.Equal(NumberOfReplicasConfiguredForPublishedSet, enabledInstanceCount);
            Assert.True(true);
        }
    }
}