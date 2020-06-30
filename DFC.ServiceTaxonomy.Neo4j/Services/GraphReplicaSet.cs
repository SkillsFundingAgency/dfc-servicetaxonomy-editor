using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Internal;

namespace DFC.ServiceTaxonomy.Neo4j.Services
{
    //todo: builder for just this for consumers that don't need multiple replicas
    public class GraphReplicaSet : IGraphReplicaSet
    {
        private readonly IGraph[] _graphInstances;

        internal GraphReplicaSet(string name, IEnumerable<IGraph> graphInstances)
        {
            Name = name;
            _graphInstances = graphInstances.ToArray();
            InstanceCount = _graphInstances.Length;
            _currentInstance = -1;
        }

        public string Name { get; }
        public int InstanceCount { get; }

        private int _currentInstance;

        public Task<List<T>> Run<T>(IQuery<T> query, int? instance = null)
        {
            // round robin might select wrong graph after overflow, but everything will still work
            //todo: unit test overflow
            instance ??= unchecked(++_currentInstance) % InstanceCount;

            return _graphInstances[instance.Value].Run(query);
        }

        public Task Run(params ICommand[] commands)
        {
            var commandTasks = _graphInstances.Select(g => g.Run(commands));
            return Task.WhenAll(commandTasks);
        }
    }
}
