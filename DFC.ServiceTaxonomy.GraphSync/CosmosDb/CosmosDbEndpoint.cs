using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbEndpoint : IEndpoint
    {
        public CosmosDbEndpoint(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }

        public async Task<List<T>> Run<T>(IQuery<T>[] queries, string databaseName, bool defaultDatabase)
        {
            try
            {
                var container = await GetContainer(databaseName);
                var returnList = new List<T>();

                foreach (var query in queries)
                {
                    var queryDefinition = new QueryDefinition(query.Query.Text);
                    var contentType = ((string)query.Query.Parameters["ContentType"]).ToLower();

                    using (FeedIterator<T> resultSetIterator = container.GetItemQueryIterator<T>(
                        queryDefinition,
                        requestOptions: new QueryRequestOptions
                        {
                            PartitionKey = new PartitionKey(contentType)
                        }))
                    {
                        while (resultSetIterator.HasMoreResults)
                        {
                            FeedResponse<T> response = await resultSetIterator.ReadNextAsync();

                            if (response.Resource.Any())
                            {
                                returnList.AddRange(response.Resource);
                            }
                        }
                    }
                }

                return returnList;
            }
            catch (Exception ex)
            {
#pragma warning disable CA2200 // Rethrow to preserve stack details
#pragma warning disable S3445 // Exceptions should not be explicitly rethrown
                throw ex; // TODO this only here for now to aid debugging
#pragma warning restore S3445 // Exceptions should not be explicitly rethrown
#pragma warning restore CA2200 // Rethrow to preserve stack details
            }
        }

        public async Task Run(ICommand[] commands, string databaseName, bool defaultDatabase)
        {
            try
            {
                var container = await GetContainer(databaseName);

                foreach (var command in commands)
                {
                    var query = command.Query.Text;

                    switch (query)
                    {
                        case "DeleteNodesByType":
                            var contentType = ((string)command.Query.Parameters["ContentType"]).ToLower();
                            var contentTypes = contentType.Split(',');

                            foreach (string contentTypeLoop in contentTypes)
                            {
                                //container.DeleteItemAsync
                            }

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
            catch (Exception ex)
            {
#pragma warning disable CA2200 // Rethrow to preserve stack details
#pragma warning disable S3445 // Exceptions should not be explicitly rethrown
                throw ex;
#pragma warning restore S3445 // Exceptions should not be explicitly rethrown
#pragma warning restore CA2200 // Rethrow to preserve stack details
            }
        }

        private static async Task ReplaceRelationshipsCommand(Container container, ICommand command)
        {
            var parameters = command.Query.Parameters;

            var uri = (string)parameters["uri"];

            var id = uri.Split('/')[uri.Split('/').Length - 1];
            var contentType = uri.Split('/')[uri.Split('/').Length - 2].ToLower();
            var relationships = (List<CommandRelationship>)parameters["relationships"];

            var iterator = container.GetItemQueryIterator<Dictionary<string, object>>(
                new QueryDefinition($"SELECT * from c where c.id = '{id}'"),
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(contentType)
                });

            var result = await iterator.ReadNextAsync();
            var item = result.Resource.SingleOrDefault();

            await UpdateIncomingRelationships(container, relationships, id, contentType, item);

            if (item == null)
            {
                item = new Dictionary<string, object>
                {
                    { "id", id },
                    { "ContentType", contentType },
                    { "_links", BuildLinksDictionary(uri) },
                };
            }

            Dictionary<string, object> _links;

            if (item.ContainsKey("_links"))
            {
                if (item["_links"] is JObject linksJobj)
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    _links = linksJobj.ToObject<Dictionary<string, object>>();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
                else
                {
                    _links = (Dictionary<string, object>)item["_links"];
                }
            }
            else
            {
                _links = BuildLinksDictionary(uri);
                item.Add("_links", _links);
            }

            foreach (var relationship in relationships)
            {
                var relationshipKey = $"cont:{relationship.RelationshipType}";
                var valuesDictionary = new Dictionary<string, object>
                {
                    { "href", relationship.DestinationNodeIdPropertyValues.FirstOrDefault() }
                };

                if (relationship.Properties != null)
                {
                    foreach (var property in relationship.Properties)
                    {
                        valuesDictionary.Add(property.Key, property.Value);
                    }
                }

                if (_links?.ContainsKey(relationshipKey) == false)
                {
                    _links.Add(relationshipKey, valuesDictionary);
                    continue;
                }

                if (_links != null)
                {
                    _links[relationshipKey] = valuesDictionary;
                }
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            item["_links"] = _links;
#pragma warning restore CS8601 // Possible null reference assignment.

            await container.UpsertItemAsync(item);
        }

        private static async Task UpdateIncomingRelationships(
            Container container,
            List<CommandRelationship> currentRelationships,
            string id,
            string contentType,
            Dictionary<string, object>? previousItem)
        {
            await HandleRemovedRelationships(container, previousItem, currentRelationships);

            foreach (var relationship in currentRelationships
                         .Where(r => !string.IsNullOrEmpty(r.DestinationNodeIdPropertyName)))
            {
                var uriLoop = (string)relationship.DestinationNodeIdPropertyValues.First();
                var idLoop = uriLoop.Split('/')[uriLoop.Split('/').Length - 1];
                var contentTypeLoop = uriLoop.Split('/')[uriLoop.Split('/').Length - 2].ToLower();

                var iteratorLoop = container.GetItemQueryIterator<Dictionary<string, object>>(
                    new QueryDefinition($"SELECT * from c where c.id = '{idLoop}'"),
                    requestOptions: new QueryRequestOptions() {PartitionKey = new PartitionKey(contentTypeLoop)});

                var resultLoop = await iteratorLoop.ReadNextAsync();
                var itemLoop = resultLoop.Resource.SingleOrDefault();

                if (itemLoop == null || !itemLoop.ContainsKey("_links") || !(itemLoop["_links"] is JObject linksJobj))
                {
                    continue;
                }

                #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                var links = linksJobj.ToObject<Dictionary<string, object>>();
                #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                var curies = ((links?["curies"] as JArray)!).ToObject<List<Dictionary<string, object>>>();
                var incomingPos = curies!.FindIndex(curie =>
                    (string)curie["name"] == "incoming");
                var incomingObj = curies.Count > incomingPos ? curies[incomingPos] : null;

                if (incomingObj == null)
                {
                    continue;
                }

                var incoming = ((incomingObj["items"] as JArray)!).ToObject<List<Dictionary<string, object>>>();
                var exists = false;

                foreach (var incomingItem in incoming!)
                {
                    contentTypeLoop = (string)incomingItem["contentType"];
                    idLoop = (string)incomingItem["id"];

                    if (idLoop == id && contentTypeLoop == contentType)
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists)
                {
                    continue;
                }

                incoming.Add(new Dictionary<string, object>
                {
                    { "contentType", contentType },
                    { "id", id },
                });

                curies[incomingPos]["items"] = incoming;
                links["curies"] = curies;
                itemLoop["_links"] = links;

                await container.UpsertItemAsync(itemLoop);
            }
        }

        private static async Task HandleRemovedRelationships(
            Container container,
            Dictionary<string, object>? previousItem,
            List<CommandRelationship> currentRelationships)
        {
            if (previousItem == null) return;

            var previousItemCompositeKey = (string)previousItem["ContentType"] + (string)previousItem["id"];
            var previousItemLinks = (previousItem!["_links"] as JObject)!.ToObject<Dictionary<string, object>>();

            var currentRelationshipsSimple = currentRelationships
                .Where(rel => rel.DestinationNodeIdPropertyValues.Any() &&
                    rel.DestinationNodeIdPropertyValues.First()?.ToString() != null)
                .Select(rel =>
                    {
                        var uri = rel.DestinationNodeIdPropertyValues.First().ToString();
                        var id = uri!.Split('/')[uri.Split('/').Length - 1];
                        var contentType = uri.Split('/')[uri.Split('/').Length - 2].ToLower();

                        return contentType + id;
                    })
                    .ToList();

            foreach (var previousItemLink in previousItemLinks!)
            {
                if (previousItemLink.Key == "self" || previousItemLink.Key == "curies")
                {
                    continue;
                }

                var dict = ((JObject)previousItemLink.Value).ToObject<Dictionary<string, object>>();
                var uri = (string)dict!["href"];

                if (string.IsNullOrEmpty(uri))
                {
                    continue;
                }

                var id = uri.Split('/')[uri.Split('/').Length - 1];
                var contentType = uri.Split('/')[uri.Split('/').Length - 2].ToLower();
                var compositeKey = contentType + id;

                // Continue if the relationship is still there
                if (currentRelationshipsSimple.Contains(compositeKey))
                {
                    continue;
                }

                // Else update their incoming relationships section
                var iterator = container.GetItemQueryIterator<Dictionary<string, object>>(
                    new QueryDefinition($"SELECT * from c where c.id = '{id}'"),
                    requestOptions: new QueryRequestOptions() {PartitionKey = new PartitionKey(contentType)});

                var result = await iterator.ReadNextAsync();
                var item = result.Resource.SingleOrDefault();

                if (item == null || !item.ContainsKey("_links") || !(item["_links"] is JObject linksJobj))
                {
                    continue;
                }

                #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                var links = linksJobj.ToObject<Dictionary<string, object>>();
                #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                var curies = ((links?["curies"] as JArray)!).ToObject<List<Dictionary<string, object>>>();
                var incomingPos = curies!.FindIndex(curie =>
                    (string)curie["name"] == "incoming");
                var incomingObj = curies.Count > incomingPos ? curies[incomingPos] : null;
                if (incomingObj == null)
                {
                    continue;
                }

                var incoming = ((incomingObj["items"] as JArray)!).ToObject<List<Dictionary<string, object>>>();
                var newIncoming = new List<Dictionary<string, object>>();

                foreach (var incomingItem in incoming!)
                {
                    var contentTypeLoop = (string)incomingItem["contentType"];
                    var idLoop = (string)incomingItem["id"];
                    var compositeKeyLoop = contentTypeLoop + idLoop;

                    if (compositeKeyLoop == previousItemCompositeKey)
                    {
                        continue;
                    }

                    newIncoming.Add(incomingItem);
                }

                curies[incomingPos]["items"] = newIncoming;
                links["curies"] = curies;
                item["_links"] = links;

                await container.UpsertItemAsync(item);
            }
        }

        private static Dictionary<string, object> BuildLinksDictionary(string uri)
        {
            var uriParts = uri.Split('/');

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


#pragma warning disable S4457 // Parameter validation in "async"/"await" methods should be wrapped
        private static async Task MergeNodeCommand(Container container, ICommand command)
#pragma warning restore S4457 // Parameter validation in "async"/"await" methods should be wrapped
        {
            var parameters = command.Query.Parameters;

            if (!parameters.ContainsKey("properties"))
            {
                throw new ArgumentException("Properties missing");
            }

            var properties = parameters["properties"] as Dictionary<string, object>;
            var uri = properties != null ? (string)properties["uri"] : string.Empty;
            var id = uri;

            var contentType = properties != null ? ((string)properties["ContentType"]).ToLower() : string.Empty;
            id = id.Split('/')[id.Split('/').Length - 1];

            var iterator = container.GetItemQueryIterator<Dictionary<string, object>>(
                new QueryDefinition($"SELECT * from c where c.id = '{id}'"),
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(contentType)
                });

            var result = await iterator.ReadNextAsync();
            var item = result.Resource.SingleOrDefault();

            if (item == null)
            {
                item = new Dictionary<string, object>
                {
                    { "id", id },
                    { "ContentType", contentType },
                    { "_links", BuildLinksDictionary(uri) },
                };
            }

            if (properties != null)
            {
                if (properties.ContainsKey("ContentType"))
                {
                    properties["ContentType"] = contentType;
                }

                foreach (var property in properties)
                {
                    if (!item.ContainsKey(property.Key))
                    {
                        item.Add(property.Key, property.Value);
                        continue;
                    }

                    item[property.Key] = property.Value;
                }
            }

            await container.UpsertItemAsync(item);
        }

        private async Task<Container> GetContainer(string databaseName)
        {
            var cosmosClient = new CosmosClient(ConnectionString);
            var db = cosmosClient.GetDatabase("dev");

            const string PartitionKeyKey = "ContentType";
            return (await db.CreateContainerIfNotExistsAsync(databaseName, $"/{PartitionKeyKey}")).Container;
        }
    }
}
