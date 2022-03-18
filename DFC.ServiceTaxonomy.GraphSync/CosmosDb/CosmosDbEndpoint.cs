﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbEndpoint : IEndpoint
    {
        private readonly ICosmosDbService _cosmosDbService;

        public string ConnectionString { get; set; }

        public CosmosDbEndpoint(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
            ConnectionString = "What is this used for?"; //TODO: need to follow this in Noe4JValidateAndRepairGraph.cs
        }

        public async Task<List<T>> Run<T>(IQuery<T>[] queries, string databaseName, bool defaultDatabase)
        {
            var container = _cosmosDbService.GetContainer(databaseName);
            var returnList = new List<T>();

            foreach (var query in queries)
            {
                var queryDefinition = new QueryDefinition(query.Query.Text);
                string contentType = ((string)query.Query.Parameters["ContentType"]).ToLower();

                using FeedIterator<T> resultSetIterator = container.GetItemQueryIterator<T>(
                    queryDefinition,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(contentType)
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
            }

            return returnList;
        }

        public async Task Run(ICommand[] commands, string databaseName, bool defaultDatabase)
        {
            var container = _cosmosDbService.GetContainer(databaseName);

            foreach (var command in commands)
            {
                string query = command.Query.Text;

                switch (query)
                {
                    case "DeleteNodesByType":
                        await DeleteNodesByTypeCommand(container, command);
                        break;
                    case "ReplaceRelationshipsCommand":
                        await ReplaceRelationshipsCommand(container, command);
                        break;
                    case "MergeNodeCommand":
                        await MergeNodeCommand(container, command);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        #pragma warning disable CS1998
        #pragma warning disable S1172
        private static async Task DeleteNodesByTypeCommand(Container container, ICommand command)
        #pragma warning restore S1172
        #pragma warning restore CS1998
        {
            string contentTypeList = ((string)command.Query.Parameters["ContentType"]).ToLower();
            string[] contentTypes = contentTypeList.Split(',');

            foreach (string contentTypeLoop in contentTypes)
            {
                //container.DeleteItemAsync
            }
        }

        private static async Task ReplaceRelationshipsCommand(Container container, ICommand command)
        {
            var commandParameters = command.Query.Parameters;

            string itemUri = (string)commandParameters["uri"];
            (string contentType, string id) = GetContentTypeAndId(itemUri);
            var itemRelationships = (List<CommandRelationship>)commandParameters["relationships"];

            var item = await GetItemFromDatabase(container, contentType, id);
            await UpdateIncomingRelationships(container, itemRelationships, id, contentType, item);

            item ??= new Dictionary<string, object>
            {
                {"id", id},
                {"ContentType", contentType},
                {"_links", BuildLinksDictionary(itemUri)},
            };

            Dictionary<string, object> itemLinks;

            if (item.ContainsKey("_links"))
            {
                if (item["_links"] is JObject linksJObject)
                {
                    itemLinks = linksJObject.ToObject<Dictionary<string, object>>()!;
                }
                else
                {
                    itemLinks = (Dictionary<string, object>)item["_links"];
                }
            }
            else
            {
                itemLinks = BuildLinksDictionary(itemUri);
                item.Add("_links", itemLinks);
            }

            foreach (var relationship in itemRelationships)
            {
                string relationshipKey = $"cont:{relationship.RelationshipType}";
                var valuesDictionary = new Dictionary<string, object>
                {
                    { "href", relationship.DestinationNodeIdPropertyValues.FirstOrDefault()! }
                };

                if (relationship.Properties != null)
                {
                    foreach ((string? key, object? value) in relationship.Properties)
                    {
                        valuesDictionary.Add(key, value);
                    }
                }

                if (itemLinks?.ContainsKey(relationshipKey) == false)
                {
                    itemLinks.Add(relationshipKey, valuesDictionary);
                    continue;
                }

                if (itemLinks == null)
                {
                    continue;
                }

                itemLinks[relationshipKey] = valuesDictionary;
            }

            item["_links"] = itemLinks!;
            await container.UpsertItemAsync(item);
        }

        #pragma warning disable S4457
        private static async Task MergeNodeCommand(Container container, ICommand command)
        #pragma warning restore S4457
        {
            var parameters = command.Query.Parameters;

            if (!parameters.ContainsKey("properties"))
            {
                throw new ArgumentException("Properties missing");
            }

            var properties = parameters["properties"] as Dictionary<string, object>;
            string uri = properties != null ? (string)properties["uri"] : string.Empty;
            string id = uri;

            string contentType = properties != null ? ((string)properties["ContentType"]).ToLower() : string.Empty;
            id = id.Split('/')[id.Split('/').Length - 1];

            var item = await GetItemFromDatabase(container, contentType, id) ?? new Dictionary<string, object>
            {
                { "id", id },
                { "ContentType", contentType },
                { "_links", BuildLinksDictionary(uri) },
            };

            if (properties == null)
            {
                await container.UpsertItemAsync(item);
                return;
            }

            if (properties.ContainsKey("ContentType"))
            {
                properties["ContentType"] = contentType;
            }

            foreach ((string? key, object? value) in properties)
            {
                if (!item.ContainsKey(key))
                {
                    item.Add(key, value);
                    continue;
                }

                item[key] = value;
            }

            await container.UpsertItemAsync(item);
        }

        private static async Task UpdateIncomingRelationships(
            Container container,
            List<CommandRelationship> itemRelationships,
            string itemId,
            string itemContentType,
            Dictionary<string, object>? itemBeforeUpdate)
        {
            await HandleRemovedRelationships(container, itemBeforeUpdate, itemRelationships);
            await HandleAddedRelationships(container, itemRelationships, itemId, itemContentType);
        }

        private static async Task HandleRemovedRelationships(
            Container container,
            Dictionary<string, object>? itemBeforeUpdate,
            List<CommandRelationship> itemRelationships)
        {
            if (itemBeforeUpdate == null) return;

            string itemContentType = (string)itemBeforeUpdate["ContentType"] + (string)itemBeforeUpdate["id"];
            var itemLinks = (itemBeforeUpdate["_links"] as JObject)!.ToObject<Dictionary<string, object>>();

            var simpleRelationships = itemRelationships
                .Where(rel =>
                    rel.DestinationNodeIdPropertyValues.Any() &&
                    rel.DestinationNodeIdPropertyValues.First().ToString() != null)
                .Select(rel =>
                    {
                        string? uri = rel.DestinationNodeIdPropertyValues.First().ToString();
                        (string contentType, string id) = GetContentTypeAndId(uri!);

                        return contentType + id;
                    })
                    .ToList();

            foreach (string? uri in itemLinks!
                 .Where(previousItemLink =>
                     previousItemLink.Key != "self" && previousItemLink.Key != "curies")
                 .Select(previousItemLink =>
                     ((JObject)previousItemLink.Value).ToObject<Dictionary<string, object>>())
                 .Select(dict => (string)dict!["href"])
                 .Where(uri => !string.IsNullOrEmpty(uri)))
            {
                (string contentType, string id) = GetContentTypeAndId(uri);
                string compositeKey = contentType + id;

                // Continue if the relationship is still there
                if (simpleRelationships.Contains(compositeKey))
                {
                    continue;
                }

                // Else update their incoming relationships section
                var relationshipItem = await GetItemFromDatabase(container, contentType, id);

                if (relationshipItem == null || !relationshipItem.ContainsKey("_links") || !(relationshipItem["_links"] is JObject linksJObject))
                {
                    continue;
                }

                var links = linksJObject.ToObject<Dictionary<string, object>>();
                var curies = ((links?["curies"] as JArray)!).ToObject<List<Dictionary<string, object>>>();
                int incomingPosition = curies!.FindIndex(curie =>
                    (string)curie["name"] == "incoming");
                var incomingObject = curies.Count > incomingPosition ? curies[incomingPosition] : null;

                if (incomingObject == null)
                {
                    continue;
                }

                var existingIncomingList = ((incomingObject["items"] as JArray)!).ToObject<List<Dictionary<string, object>>>();
                var newIncomingList = new List<Dictionary<string, object>>();

                foreach (var incomingItem in existingIncomingList!)
                {
                    string contentTypeLoop = (string)incomingItem["contentType"];
                    string idLoop = (string)incomingItem["id"];
                    string compositeKeyLoop = contentTypeLoop + idLoop;

                    // It's linked to the parent still, so keep it
                    if (compositeKeyLoop == itemContentType)
                    {
                        continue;
                    }

                    newIncomingList.Add(incomingItem);
                }

                curies[incomingPosition]["items"] = newIncomingList;
                links["curies"] = curies;
                relationshipItem["_links"] = links;

                await container.UpsertItemAsync(relationshipItem);
            }
        }

         private static async Task HandleAddedRelationships(
            Container container,
            List<CommandRelationship> itemRelationships,
            string itemId,
            string itemContentType)
        {
            foreach (var relationship in itemRelationships
                .Where(r => !string.IsNullOrEmpty(r.DestinationNodeIdPropertyName)))
            {
                string relationshipUri = (string)relationship.DestinationNodeIdPropertyValues.First();
                (string relationshipContentType, string relationshipId) = GetContentTypeAndId(relationshipUri);
                var relationshipItem = await GetItemFromDatabase(container, relationshipContentType, relationshipId);

                if (relationshipItem == null || !relationshipItem.ContainsKey("_links") || !(relationshipItem["_links"] is JObject linksJObject))
                {
                    continue;
                }

                var links = linksJObject.ToObject<Dictionary<string, object>>();

                var curies = ((links?["curies"] as JArray)!).ToObject<List<Dictionary<string, object>>>();
                int incomingPosition = curies!.FindIndex(curie =>
                    (string)curie["name"] == "incoming");
                var incomingObject = curies.Count > incomingPosition ? curies[incomingPosition] : null;

                if (incomingObject == null)
                {
                    throw new MissingFieldException("Incoming collection missing");
                }

                var incomingList = (incomingObject["items"] as JArray)!.ToObject<List<Dictionary<string, object>>>();
                bool itemLinkAlreadyExists = false;

                foreach (var incomingItem in incomingList!)
                {
                    string incomingContentType = (string)incomingItem["contentType"];
                    string incomingId = (string)incomingItem["id"];

                    itemLinkAlreadyExists = incomingId == itemId && incomingContentType == itemContentType;

                    if (itemLinkAlreadyExists)
                    {
                        break;
                    }
                }

                if (itemLinkAlreadyExists)
                {
                    continue;
                }
                incomingList.Add(new Dictionary<string, object>
                {
                    { "contentType", itemContentType },
                    { "id", itemId },
                });

                curies[incomingPosition]["items"] = incomingList;
                links["curies"] = curies;
                relationshipItem["_links"] = links;

                await container.UpsertItemAsync(relationshipItem);
            }
        }



        private static (string, string) GetContentTypeAndId(string uri)
        {
            string id = uri.Split('/')[uri.Split('/').Length - 1];
            string contentType = uri.Split('/')[uri.Split('/').Length - 2].ToLower();

            return (contentType, id);
        }

        private static async Task<Dictionary<string, object>?> GetItemFromDatabase(Container container, string contentType, string id)
        {
            var iteratorLoop = container.GetItemQueryIterator<Dictionary<string, object>>(
                new QueryDefinition($"SELECT * FROM c WHERE c.id = '{id}'"),
                requestOptions: new QueryRequestOptions {PartitionKey = new PartitionKey(contentType)});

            var result = await iteratorLoop.ReadNextAsync();
            return result.Resource.SingleOrDefault();
        }

        private static Dictionary<string, object> BuildLinksDictionary(string uri)
        {
            string[] uriParts = uri.Split('/');

            return new Dictionary<string, object>
            {
                { "self", uri },
                { "curies", new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "name", "cont" },
                            { "href", $"https://{uriParts[2]}/{uriParts[3]}/{uriParts[4]}" }
                        },
                        new Dictionary<string, object>
                        {
                            { "name", "incoming" },
                            { "items", new List<KeyValuePair<string, object>>() }
                        },
                    }
                }
            };
        }
    }
}
