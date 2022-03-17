using System;
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
            try
            {
                var container = _cosmosDbService.GetContainer(databaseName);
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
                var container = _cosmosDbService.GetContainer(databaseName);

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

        private static Dictionary<string, object> BuildLinksDictionary(string uri)
        {
            var uriParts = uri.Split('/');

            return new Dictionary<string, object>
            {
                { "self", uri },
                { "curies", new Dictionary<string, object>[]
                    {
                         new Dictionary<string, object>
                         {
                             { "name", "cont" },
                             { "href", $"https://{uriParts[2]}/{uriParts[3]}/{uriParts[4]}" }
                         }
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
    }
}
