using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Exceptions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

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


        public async Task DeleteIncomingRelationshipAsync(Container container, string contentType, string id, string relationshipId)
        {
            var relationshipItem = await GetContentItemFromDatabase(container, contentType, id);
            // if there is no relationship item or it doesn;t have a _links section we don't need to bother updating
            if (relationshipItem == null || !relationshipItem.ContainsKey("_links") || !(relationshipItem["_links"] is JObject linksJObject))
            {
                return;
            }
            // also if there is no incoming section we don't need to bother updating
            var incominglist = linksJObject["curies"].FirstOrDefault(a => (string)a["name"]! == "incoming") as JObject;
            if (incominglist == null)
            {
                return;
            }
            // amend the list of items to remove relationship
            var incomingListItems = incominglist["items"].Where(l => (string)l["contentType"]! + (string)l["id"]! != relationshipId).ToList();
            incominglist["items"] = incomingListItems.Any() ? new JArray(incomingListItems) : new JArray();

            await container.UpsertItemAsync(relationshipItem);
        }


        public async Task<Dictionary<string, object>?> GetContentItemFromDatabase(Container container, string contentType, string id)
        {
            var iteratorLoop = container.GetItemQueryIterator<Dictionary<string, object>>(
                new QueryDefinition($"SELECT * FROM c WHERE c.id = '{id}'"),
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(contentType) });

            var result = await iteratorLoop.ReadNextAsync();
            return result.Resource.SingleOrDefault();
        }
    }
}
