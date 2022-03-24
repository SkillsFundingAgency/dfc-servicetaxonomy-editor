using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbEndpoint : IEndpoint
    {
        private readonly ICosmosDbService _cosmosDbService;

        public string Name { get; set; }

        public CosmosDbEndpoint(ICosmosDbService cosmosDbService, string endpointName)
        {
            _cosmosDbService = cosmosDbService;
            Name = endpointName; // TODO: need to follow this in Noe4JValidateAndRepairGraph.cs
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
                    case "DeleteNodeCommand":
                        await DeleteNodeCommand(container, command);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static Task DeleteNodeCommand(Container container, ICommand command)
        {
            // find outgoing relations
            // remove these from related items
            // delete unnecessary related items (html, htmlshared)
            // delete item
            throw new NotImplementedException();
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

        private async Task ReplaceRelationshipsCommand(Container container, ICommand command)
        {
            var commandParameters = command.Query.Parameters;
            
            string itemUri = (string)commandParameters["uri"];
            (string contentType, string id) = GetContentTypeAndId(itemUri);
            var itemRelationships = (List<CommandRelationship>)commandParameters["relationships"];

            var item = await _cosmosDbService.GetContentItemFromDatabase(container, contentType, id);
            await UpdateIncomingRelationships(container, itemRelationships, id, contentType, item);

            item ??= new Dictionary<string, object>
            {
                {"id", id},
                {"ContentType", contentType},
                {"_links", BuildLinksDictionary(itemUri)},
            };

            Dictionary<string, object> itemLinks = BuildLinksDictionary(itemUri);

            if (item.ContainsKey("_links"))
            {
                if (item["_links"] is JObject linksJObject)
                {
                    var linksDictionary = linksJObject.ToObject<Dictionary<string, object>>()!;
                    itemLinks["curies"] = linksDictionary["curies"];
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

            var relationships = itemRelationships.Select(r => ExtractRelationship(r));
            var keys = relationships.Select(r => r.Item1).Distinct();
            foreach (var key in keys)
            {
                var keyValues = relationships.Where(r => r.Item1 == key).Select(r => r.Item2).ToArray();
                if (keyValues.Count() == 1)
                {
                    itemLinks.Add(key, keyValues.First());
                }
                else
                {
                    itemLinks.Add(key, keyValues);
                }
            }

            item["_links"] = itemLinks!;
            await container.UpsertItemAsync(item);
        }

        private static (string, JObject) ExtractRelationship(CommandRelationship relationship)
        {
            var relationshipKey = $"cont:{relationship.RelationshipType}";
            var valuesDictionary = new JObject();
            valuesDictionary.Add(new JProperty("href", ((string)relationship.DestinationNodeIdPropertyValues.FirstOrDefault()!).ExtactCurieHref()));

            if (relationship.Properties != null)
            {
                foreach ((string? key, object? value) in relationship.Properties)
                {
                    valuesDictionary.Add(new JProperty(key, value));
                }
            }
            return(relationshipKey, valuesDictionary);
        }

#pragma warning disable S4457
        private async Task MergeNodeCommand(Container container, ICommand command)
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

            var item = await _cosmosDbService.GetContentItemFromDatabase(container, contentType, id) ?? new Dictionary<string, object>
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

        private static string GetRelationshipId(CommandRelationship commandRelationship)
        {
            if(!(commandRelationship.DestinationNodeIdPropertyValues.FirstOrDefault() is string uri))
            {
                return string.Empty;
            }
            if(string.IsNullOrWhiteSpace(uri))
            {
                return string.Empty;
            }
            (string contentType, string id) = GetContentTypeAndId(uri);
            return $"{contentType}{id}";
        }

        private async Task HandleRemovedRelationships(
            Container container,
            Dictionary<string, object>? itemBeforeUpdate,
            List<CommandRelationship> itemRelationships)
        {
            if (itemBeforeUpdate == null) return;

            string itemId = (string)itemBeforeUpdate["ContentType"] + (string)itemBeforeUpdate["id"];
            var existingItemRelationships = (itemBeforeUpdate["_links"] as JObject)!.GetLinks().SelectMany(l => l.Value.Select(v => GetContentTypeAndId(v)));
            var newRelationshipIds = itemRelationships.Select(GetRelationshipId).Where(r => string.IsNullOrWhiteSpace(r)).ToList();

            // filter for any existing relationships that are no longer present in the new relationships
            foreach (var item in existingItemRelationships.Where(i => newRelationshipIds.All(n => $"{i.Item1}{i.Item2}" != n)))
            {
                await _cosmosDbService.DeleteIncomingRelationshipAsync(container, item.Item1, item.Item2, itemId);
            }
        }


        private async Task HandleAddedRelationships(
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
                var relationshipItem = await _cosmosDbService.GetContentItemFromDatabase(container, relationshipContentType, relationshipId);

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
