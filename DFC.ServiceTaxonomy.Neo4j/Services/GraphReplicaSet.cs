using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private protected readonly Graph[] _graphInstances;
        private protected readonly int? _limitToGraphInstance;

        internal GraphReplicaSet(string name, IEnumerable<Graph> graphInstances, int? limitToGraphInstance = null)
        {
            Name = name;
            _graphInstances = graphInstances.ToArray();
            _enabledInstanceCount = InstanceCount = _graphInstances.Length;
            //todo: check in range
            _limitToGraphInstance = limitToGraphInstance;
            _currentInstance = -1;
        }

        public string Name { get; }
        public int InstanceCount { get; }
        public int EnabledInstanceCount => (int)Interlocked.Read(ref _enabledInstanceCount);

        protected long _enabledInstanceCount;
        private int _currentInstance;

        public Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
            Graph? graphInstance;

            if (_limitToGraphInstance != null)
            {
                graphInstance = _graphInstances[_limitToGraphInstance.Value];
                if (!graphInstance.Enabled)
                    throw new InvalidOperationException($"GraphReplicaSet in single replica mode, but replica #{_limitToGraphInstance.Value} is disabled. ");
            }
            else if (EnabledInstanceCount < InstanceCount)
            {
                // should we even allow all replicas to be disabled?
                if (EnabledInstanceCount == 0)
                    throw new InvalidOperationException("No enabled replicas to run query against.");

                //todo: do we set _currentInstance to 0 when any graph is enabled/disabled?
                if (_currentInstance >= EnabledInstanceCount)
                {
                    _currentInstance = 0;
                    graphInstance = _graphInstances[0];
                }
                else
                {
                    int instance = unchecked(++_currentInstance) % EnabledInstanceCount;
                    //todo: how to do this safely without excessive locking
                    graphInstance = _graphInstances.Where(g => g.Enabled).Skip(instance).First();
                }
            }
            else
            {
                // fast path, simple round-robin read
                graphInstance = _graphInstances[unchecked(++_currentInstance) % InstanceCount];
            }

            return graphInstance.Run(queries);
        }

        public Task Run(params ICommand[] commands)
        {
            //todo: need to update run command too

            if (_limitToGraphInstance != null)
                return _graphInstances[_limitToGraphInstance.Value].Run(commands);

            var commandTasks = _graphInstances.Select(g => g.Run(commands));
            return Task.WhenAll(commandTasks);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
