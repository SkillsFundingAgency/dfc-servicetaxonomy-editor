using System.Collections.Concurrent;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Exceptions;
using Microsoft.Azure.Cosmos;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly ConcurrentDictionary<string, Container> _containers;

        public CosmosDbService(ConcurrentDictionary<string, Container> containers)
        {
            _containers = containers;
        }

        public Container GetContainer(string name)
        {
            if(!_containers.ContainsKey(name))
            {
                throw new GraphClusterConfigurationErrorException($"'{name}' endpoint has not been created configured. Please check the configuration.");
            }
            return _containers[name];
        }
    }
}
