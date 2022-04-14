using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Commands;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbEndpoint : IEndpoint
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly ILogger _logger;

        public string Name { get; set; }

        public CosmosDbEndpoint(ICosmosDbService cosmosDbService, string endpointName, ILogger logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
            Name = endpointName; // TODO: need to follow this in Noe4JValidateAndRepairGraph.cs
        }

        public async Task<List<T>> Run<T>(IQuery<T>[] queries, string databaseName, bool defaultDatabase)
        {
            var returnList = new List<T>();

            foreach (var query in queries.Where(q => q.Query.QueryDefinition != null && q.Query.QueryRequestOptions != null))
            {
                var queryList = await _cosmosDbService.QueryContentItemsAsync<T>(databaseName, query.Query.QueryDefinition!, query.Query.QueryRequestOptions!);
                returnList.AddRange(queryList);
            }

            return returnList;
        }

        public async Task Run(ICommand[] commands, string databaseName, bool defaultDatabase)
        {
            foreach (var command in commands)
            {
                string query = command.Query.Text;
                _logger.LogWarning($"{query} called");

                switch (query)
                {
                    case "DeleteNodeCommand":
                        await DeleteNodeCommand(databaseName, command);
                        break;
                    case "DeleteNodesByType":
                        await DeleteNodesByTypeCommand(databaseName, command);
                        break;
                    case "DeleteRelationships":
                        await DeleteRelationshipsCommand(databaseName, command);
                        break;
                    case "MergeNodeCommand":
                        await MergeNodeCommand(databaseName, command);
                        break;
                    case "ReplaceRelationshipsCommand":
                        await ReplaceRelationshipsCommand(databaseName, command);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private Task DeleteNodeCommand(string databaseName, ICommand command)
        {
            var commandParameters = command.Query.Parameters;

            string itemUri = (string)commandParameters["uri"];
            (string contentType, Guid id) = DocumentHelper.GetContentTypeAndId(itemUri);
            return _cosmosDbService.DeleteItemAsync(databaseName, contentType, id);
        }

        private async Task DeleteNodesByTypeCommand(string databaseName, ICommand command)
        {
            var contentTypeList = ((string)command.Query.Parameters["ContentType"]).ToLower();
            var contentTypes = contentTypeList.Split(',').Where(ct => !string.IsNullOrEmpty(ct)).ToList();

            foreach (string contentType in contentTypes.Where(ct => !string.IsNullOrEmpty(ct)))
            {
                var resultList =
                    await _cosmosDbService.QueryContentItemsAsync<Dictionary<string, object>>(databaseName, "select * from c",
                        contentType);

                foreach (var item in resultList)
                {
                    var id = new Guid((string)item["id"]);
                    await _cosmosDbService.DeleteItemAsync(databaseName, contentType, id);
                }
            }
        }

        private async Task DeleteRelationshipsCommand(string databaseName, ICommand command)
        {
            var commandParameters = command.Query.Parameters;
            string itemUri = (string)commandParameters["sourceIdPropertyValue"];
            (string contentType, Guid id) = DocumentHelper.GetContentTypeAndId(itemUri);
            var item = await _cosmosDbService.GetContentItemFromDatabase(databaseName, contentType, id);
            var existingItemRelationships = (item!["_links"] as JObject)!.GetLinks().SelectMany(l => l.Value.Select(DocumentHelper.GetContentTypeAndId));

            // Since the move to cosmos the destination node source ids don't appear to be populated
            if (command is CosmosDbDeleteRelationshipsCommand deleteRelationshipsCommand)
            {
                var relationshipContentTypes = deleteRelationshipsCommand.Relationships
                    .SelectMany(r => r.DestinationNodeLabels.Where(nl => !nl.Equals("Resource", StringComparison.InvariantCultureIgnoreCase))).ToList();
                foreach (var existingItemRelationship in existingItemRelationships.Where(er => relationshipContentTypes.Any(rct => rct.Equals(er.Item1, StringComparison.InvariantCultureIgnoreCase))))
                {
                    if (deleteRelationshipsCommand.DeleteDestinationNodes)
                    {
                        await _cosmosDbService.DeleteItemAsync(databaseName, existingItemRelationship.Item1, existingItemRelationship.Item2);
                    }
                    else
                    {
                        await _cosmosDbService.DeleteIncomingRelationshipAsync(databaseName,
                            existingItemRelationship.Item1, existingItemRelationship.Item2, $"{contentType}{id}");
                    }
                }
            }
        }

        private async Task MergeNodeCommand(string databaseName, ICommand command)
        {
            var properties = command.Query.Parameters["properties"] as Dictionary<string, object>;
            if (properties == null || !properties.ContainsKey("uri"))
            {
                return;
            }
            var itemUri = (string)properties["uri"];
            (string contentType, Guid id) = DocumentHelper.GetContentTypeAndId(itemUri);

            var item = await _cosmosDbService.GetContentItemFromDatabase(databaseName, contentType, id) ?? new Dictionary<string, object>
            {
                { "id", id },
                { "ContentType", contentType },
                { "_links", BuildLinksDictionary(itemUri) },
            };

            foreach ((string? key, object? value) in properties.Where(p => !p.Key.Equals("ContentType")))
            {
                if (!item.ContainsKey(key))
                {
                    item.Add(key, value);
                    continue;
                }

                item[key] = value;
            }

            await _cosmosDbService.UpdateItemAsync(databaseName, item);
        }


        private async Task ReplaceRelationshipsCommand(string databaseName, ICommand command)
        {
            var commandParameters = command.Query.Parameters;
            
            string itemUri = (string)commandParameters["uri"];
            (string contentType, Guid id) = DocumentHelper.GetContentTypeAndId(itemUri);
            var item = await _cosmosDbService.GetContentItemFromDatabase(databaseName, contentType, id);
            var itemRelationships = (List<CommandRelationship>)commandParameters["relationships"];


            await HandleRemovedRelationships(databaseName, item, itemRelationships);
            await HandleAddedRelationships(databaseName, itemRelationships, id, contentType);

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

            var relationships = itemRelationships.Select(ExtractRelationship);
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
            await _cosmosDbService.UpdateItemAsync(databaseName, item);
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
            (string contentType, Guid id) = DocumentHelper.GetContentTypeAndId(uri);
            return $"{contentType}{id}";
        }

        private async Task HandleRemovedRelationships(
            string databaseName,
            Dictionary<string, object>? itemBeforeUpdate,
            List<CommandRelationship> itemRelationships)
        {
            if (itemBeforeUpdate == null) return;

            string itemId = (string)itemBeforeUpdate["ContentType"] + (string)itemBeforeUpdate["id"];
            var existingItemRelationships = (itemBeforeUpdate["_links"] as JObject)!.GetLinks().SelectMany(l => l.Value.Select(DocumentHelper.GetContentTypeAndId));
            var newRelationshipIds = itemRelationships.Select(GetRelationshipId).Where(r => string.IsNullOrWhiteSpace(r)).ToList();

            // filter for any existing relationships that are no longer present in the new relationships
            foreach (var item in existingItemRelationships.Where(i => newRelationshipIds.All(n => $"{i.Item1}{i.Item2}" != n)))
            {
                await _cosmosDbService.DeleteIncomingRelationshipAsync(databaseName, item.Item1, item.Item2, itemId);
            }
        }


        private async Task HandleAddedRelationships(
            string databaseName,
            List<CommandRelationship> itemRelationships,
            Guid itemId,
            string itemContentType)
        {
            foreach (var relationship in itemRelationships
                .Where(r => !string.IsNullOrEmpty(r.DestinationNodeIdPropertyName)))
            {
                string relationshipUri = (string)relationship.DestinationNodeIdPropertyValues.First();
                (string relationshipContentType, Guid relationshipId) = DocumentHelper.GetContentTypeAndId(relationshipUri);
                var relationshipItem = await _cosmosDbService.GetContentItemFromDatabase(databaseName, relationshipContentType, relationshipId);

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
                    var incomingId = new Guid((string)incomingItem["id"]);

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

                await _cosmosDbService.UpdateItemAsync(databaseName, relationshipItem);
            }
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
