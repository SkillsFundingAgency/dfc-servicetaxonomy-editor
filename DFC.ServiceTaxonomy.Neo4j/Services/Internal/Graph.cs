using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class Graph : IGraph
    {
        public string GraphName { get; }
        public bool DefaultGraph { get; }
        public int Instance { get; }
        public IGraphReplicaSetLowLevel GraphReplicaSetLowLevel { get; internal set; }

        private readonly INeoEndpoint _endpoint;

        public Graph(INeoEndpoint endpoint, string graphName, bool defaultGraph, int instance)
        {
            _endpoint = endpoint;
            GraphName = graphName;
            DefaultGraph = defaultGraph;
            Instance = instance;

            // GraphReplicaSet will set this as part of the build process, before any consumer gets an instance of this class
            GraphReplicaSetLowLevel = default!;
        }

        public Task<List<T>> Run<T>(IQuery<T> query)
        {
            return _endpoint.Run(query, GraphName, DefaultGraph);
        }

        public Task Run(params ICommand[] commands)
        {
            return _endpoint.Run(commands, GraphName, DefaultGraph);
        }

        public IGraphReplicaSetLowLevel GetReplicaSetLimitedToThisGraph()
        {
            return GraphReplicaSetLowLevel.CloneLimitedToGraphInstance(Instance);
        }
    }
}
