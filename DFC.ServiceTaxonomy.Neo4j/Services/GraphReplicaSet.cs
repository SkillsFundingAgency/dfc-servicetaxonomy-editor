﻿using System;
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
            _instanceCounter = -1;
        }

        public string Name { get; }
        public int InstanceCount { get; }
        public int EnabledInstanceCount => (int)Interlocked.Read(ref _enabledInstanceCount);

        protected long _enabledInstanceCount;
        private int _instanceCounter;

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
                if (EnabledInstanceCount == 0)
                    throw new InvalidOperationException("No enabled replicas to run query against.");

                int enabledInstance = unchecked(++_instanceCounter) % EnabledInstanceCount;

                //todo: how to do this safely without excessive locking
                graphInstance = _graphInstances.Where(g => g.Enabled).Skip(enabledInstance).First();
            }
            else
            {
                // fast path, simple round-robin read
                graphInstance = _graphInstances[unchecked(++_instanceCounter) % InstanceCount];
            }

            return graphInstance.Run(queries);
        }

        public Task Run(params ICommand[] commands)
        {
            if (_limitToGraphInstance != null)
            {
                Graph graph = _graphInstances[_limitToGraphInstance.Value];
                if (!graph.Enabled)
                    throw new InvalidOperationException($"GraphReplicaSet in single replica mode, but replica #{_limitToGraphInstance.Value} is disabled. ");

                return graph.Run(commands);
            }

            IEnumerable<Graph> commandGraphs = EnabledInstanceCount < InstanceCount
                ? _graphInstances.Where(g => g.Enabled)
                : _graphInstances;

            return Task.WhenAll(commandGraphs.Select(g => g.Run(commands)));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
