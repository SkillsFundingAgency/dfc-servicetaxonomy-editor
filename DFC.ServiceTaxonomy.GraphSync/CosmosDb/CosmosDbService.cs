using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Helpers;
using Microsoft.Azure.Cosmos;
using static DFC.ServiceTaxonomy.GraphSync.Helpers.DocumentHelper;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly ConcurrentDictionary<string, Container> _containers;

        public CosmosDbService(ConcurrentDictionary<string, Container> containers)
        {
            if (containers == null || containers.IsEmpty)
            {
                throw new GraphClusterConfigurationErrorException("No containers to setup");
            }

            _containers = containers;
        }

        private Container GetContainer(string name)
        {
            if (!_containers.ContainsKey(name))
            {
                throw new GraphClusterConfigurationErrorException($"'{name}' endpoint has not been configured. Please check the configuration.");
            }

            return _containers[name];
        }

        public Task DeleteIncomingRelationshipAsync(string databaseName, string contentType, Guid id, string relationshipId)
        {
            var container = GetContainer(databaseName);
            return DeleteIncomingRelationshipAsync(container, contentType, id, relationshipId);
        }

        private async Task DeleteIncomingRelationshipAsync(Container container, string contentType, Guid id, string relationshipId)
        {
            var relationshipItem = await GetContentItemFromDatabase(container, contentType, id);

            // If there is no relationship item or it doesn't have a _links section we don't need to bother updating
            if (relationshipItem?.ContainsKey("_links") != true)
            {
                return;
            }

            // Also if there is no incoming section we don't need to bother updating
            var links = SafeCastToDictionary(relationshipItem["_links"]);
            var curies = SafeCastToList(links["curies"]);

            int incomingPosition = curies.FindIndex(curie =>
                (string)curie["name"] == "incoming");
            var incomingObject = curies.Count > incomingPosition ? curies[incomingPosition] : null;

            if (incomingObject == null)
            {
                return;
            }

            var incomingList = SafeCastToList(incomingObject["items"]);

            // amend the list of items to remove relationship
            var incomingListItems = incomingList
                .Where(l => (string)l["contentType"] + (string)l["id"] != relationshipId)
                .ToList();

            curies[incomingPosition]["items"] = incomingListItems;
            links["curies"] = curies;
            relationshipItem["_links"] = links;

            await container.UpsertItemAsync(relationshipItem);
        }

        public async Task DeleteItemAsync(string databaseName, string contentType, Guid id)
        {
            var container = GetContainer(databaseName);
            var contentItem = await GetContentItemFromDatabase(container, contentType, id);
            if (contentItem == null) return;

            // find outgoing relations to other content items
            var existingItemRelationships = SafeCastToDictionary(contentItem["_links"])
                .GetLinks()
                .SelectMany(l => l.Value.Select(DocumentHelper.GetContentTypeAndId));

            // remove an incoming relationship to the content item to be deleted from these related content items
            foreach ((string? ContentType, Guid Id) relationship in existingItemRelationships)
            {
                await DeleteIncomingRelationshipAsync(container, relationship.ContentType!, relationship.Id, $"{contentType}{id}");
            }

            // delete content item
            await container.DeleteItemAsync<Dictionary<string, object>>(id.ToString(), new PartitionKey(contentType.ToLower()));
        }

        public Task UpdateItemAsync(string databaseName, Dictionary<string, object> item)
        {
            var container = GetContainer(databaseName);
            return container.UpsertItemAsync(item);
        }

        public Task<Dictionary<string, object>?> GetContentItemFromDatabase(string databaseName, string contentType, Guid id)
        {
            var container = GetContainer(databaseName);
            return GetContentItemFromDatabase(container, contentType, id);
        }

        private async Task<Dictionary<string, object>?> GetContentItemFromDatabase(Container container, string contentType, Guid id)
        {
            var iteratorLoop = container.GetItemQueryIterator<Dictionary<string, object>>(
                new QueryDefinition($"SELECT * FROM c WHERE c.id = @id0").WithParameter("@id0", id.ToString()),
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(contentType) });

            var result = await iteratorLoop.ReadNextAsync();
            return result.Resource.SingleOrDefault();
        }

        public async Task<List<T>> QueryContentItemsAsync<T>(string databaseName, QueryDefinition queryDefinition, QueryRequestOptions queryRequestOptions)
        {
            var returnList = new List<T>();
            var container = GetContainer(databaseName);

            using FeedIterator<T> resultSetIterator = container.GetItemQueryIterator<T>(
                queryDefinition,
                requestOptions: queryRequestOptions);

            while (resultSetIterator.HasMoreResults)
            {
                FeedResponse<T> response = await resultSetIterator.ReadNextAsync();

                if (!response.Resource.Any())
                {
                    continue;
                }

                returnList.AddRange(response.Resource);
            }

            return returnList;
        }

        public async Task<List<T>> QueryContentItemsAsync<T>(string databaseName, string query, string contentType)
        {
            var returnList = new List<T>();
            var queryDefinition = new QueryDefinition(query);
            var container = GetContainer(databaseName);

            using FeedIterator<T> resultSetIterator = container.GetItemQueryIterator<T>(
                queryDefinition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(contentType),
                    MaxItemCount = int.MaxValue
                });

            while (resultSetIterator.HasMoreResults)
            {
                FeedResponse<T> response = await resultSetIterator.ReadNextAsync();

                if (!response.Resource.Any())
                {
                    continue;
                }

                returnList.AddRange(response.Resource);
            }

            return returnList;
        }
    }
}
