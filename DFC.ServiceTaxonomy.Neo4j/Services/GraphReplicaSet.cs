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
        private protected readonly IGraph[] _graphInstances;

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

        // alternative is to expose IGraph instances, so validate doesn't have to use for loop
        // but want to keep IGraph hidden from consumers, to allow more freedom for refactoring
        // (and validate is low-level anyway).
        // would be nice to hide low-level options like running a command against a particular instance
        // from normal consumers to stop them fscking up, using something like c++ friends
        public Task Run(params ICommand[] commands)
        {
            var commandTasks = _graphInstances.Select(g => g.Run(commands));
            return Task.WhenAll(commandTasks);
        }
    }
}
