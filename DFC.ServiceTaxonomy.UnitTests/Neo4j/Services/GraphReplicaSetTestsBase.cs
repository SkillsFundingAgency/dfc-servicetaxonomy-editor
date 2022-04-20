using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Services
{
    public class GraphReplicaSetTestsBase
    {
        internal readonly ITestOutputHelper TestOutputHelper;
        internal List<DataSync.Models.DataSync> GraphInstances { get; set; }
        internal ILogger Logger { get; set; }
        internal IQuery<int> Query { get; set; }
        internal ICommand Command { get; set; }

        internal const string ReplicaSetName = "ReplicaSetName";
        internal const int NumberOfGraphInstances = 10;

        protected GraphReplicaSetTestsBase(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            GraphInstances = new List<DataSync.Models.DataSync>();
            for (int graphInstanceOrdinal = 0; graphInstanceOrdinal < NumberOfGraphInstances; ++graphInstanceOrdinal)
            {
                GraphInstances.Add(A.Fake<DataSync.Models.DataSync>());
            }
            Logger = A.Fake<ILogger>();

            Query = A.Fake<IQuery<int>>();
            Command = A.Fake<ICommand>();
        }
    }
}
