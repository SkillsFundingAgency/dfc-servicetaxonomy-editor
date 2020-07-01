using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Internal
{
    internal class Graph : IGraph
    {
        public string GraphName { get; }
        public bool DefaultGraph { get; }
        public int Instance { get; }
        public IGraphReplicaSet GraphReplicaSet { get; internal set; }

        private readonly INeoEndpoint _endpoint;

        public Graph(INeoEndpoint endpoint, string graphName, bool defaultGraph, int instance)
        {
            _endpoint = endpoint;
            GraphName = graphName;
            DefaultGraph = defaultGraph;
            Instance = instance;
        }

        public Task<List<T>> Run<T>(IQuery<T> query)
        {
            return _endpoint.Run(query, GraphName, DefaultGraph);
        }

        public Task Run(params ICommand[] commands)
        {
            return _endpoint.Run(commands, GraphName, DefaultGraph);
        }
    }
}
