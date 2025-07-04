﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Commands;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using static DFC.ServiceTaxonomy.GraphSync.Helpers.DocumentHelper;

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
            Name = endpointName;
        }

        public async Task<List<T>> Run<T>(IQuery<T>[] queries, string databaseName, bool defaultDatabase)
        {
            var returnList = new List<T>();

            foreach (var query in queries.Where(q => q.Query.QueryDefinition != null && q.Query.QueryRequestOptions != null))
            {
                var queryList = await _cosmosDbService.QueryContentItemsAsync<T>(
                    databaseName,
                    query.Query.QueryDefinition!,
                    query.Query.QueryRequestOptions!);

                returnList.AddRange(queryList);
            }

            return returnList;
        }

        public async Task Run(ICommand[] commands, string databaseName, bool defaultDatabase)
        {
            foreach (var command in commands)
            {
                string queryType = command.Query.Text;
                _logger.LogInformation("{QueryType} called", queryType);

                switch (queryType)
                {
                    case "DeleteNodeCommand":
                    case "UnpublishNodeCommand":
                        await DeleteNodeCommand(databaseName, command, queryType == "DeleteNodeCommand");
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

        private async Task DeleteNodeCommand(string databaseName, ICommand command, bool blockOnIncomingLinks)
        {
            var commandParameters = command.Query.Parameters;
            string itemUri = (string)commandParameters["uri"];
            (_, Guid id) = GetContentTypeAndId(itemUri);
            var contentType = ((CosmosDbDeleteNodeCommand)command).NodeLabels.First().ToLower();

            var itemInCosmos = await _cosmosDbService.GetContentItemFromDatabase(databaseName, contentType, id);

            if (itemInCosmos == null)
            {
                // If its not in Cosmos, we don't need to try and delete it
                return;
            }

            var incomingLinks = GetIncomingLinks(itemInCosmos);

            if (blockOnIncomingLinks && incomingLinks.Any())
            {
                if (!contentType.Contains("apprenticeshipstandard"))
                {
                    throw new ValidationException($"This content is referenced by {incomingLinks.Count} other {GetItemOrItems(incomingLinks.Count)}");
                }

                await RemoveItemFromIncomingLinks(databaseName, incomingLinks, id);
            }

            await _cosmosDbService.DeleteItemAsync(databaseName, contentType, id);
        }

        private async Task DeleteNodesByTypeCommand(string databaseName, ICommand command)
        {
            var contentTypeList = ((string)command.Query.Parameters["ContentType"]).ToLower();
            var contentTypes = contentTypeList.Split(',').Where(ct => !string.IsNullOrEmpty(ct)).ToList();

            foreach (string contentType in contentTypes.Where(ct => !string.IsNullOrEmpty(ct)))
            {
                var resultList =
                    await _cosmosDbService.QueryContentItemsAsync<Dictionary<string, object>>(databaseName, "SELECT * FROM c",
                        contentType);

                foreach (var item in resultList)
                {
                    var incomingLinks = GetIncomingLinks(item);

                    if (incomingLinks.Any())
                    {
                        var title = (string)item["skos__prefLabel"];

                        throw new ValidationException(
                            $"Content {title} is referenced by {incomingLinks.Count} other {GetItemOrItems(incomingLinks.Count)}");
                    }

                    var id = new Guid((string)item["id"]);
                    await _cosmosDbService.DeleteItemAsync(databaseName, contentType, id);
                }
            }
        }

        private async Task DeleteRelationshipsCommand(string databaseName, ICommand command)
        {
            var commandParameters = command.Query.Parameters;
            string itemUri = (string)commandParameters["sourceIdPropertyValue"];
            (string? contentType, Guid id) = GetContentTypeAndId(itemUri);

            var item = await _cosmosDbService.GetContentItemFromDatabase(databaseName, contentType!, id);
            var existingItemRelationships = SafeCastToDictionary(item!["_links"])
                .GetLinks()
                .SelectMany(l => l.Value.Select(GetContentTypeAndId));

            // Since the move to cosmos the destination node source ids don't appear to be populated
            if (command is CosmosDbDeleteRelationshipsCommand deleteRelationshipsCommand)
            {
                var relationshipContentTypes = deleteRelationshipsCommand.Relationships
                    .SelectMany(r => r.DestinationNodeLabels.Where(nl => !nl.Equals("Resource", StringComparison.InvariantCultureIgnoreCase))).ToList();

                foreach ((string? relationshipContentType, Guid relationshipId) in existingItemRelationships
                             .Where(er => relationshipContentTypes.Any(rct => rct.Equals(er.ContentType, StringComparison.InvariantCultureIgnoreCase))))
                {
                    if (deleteRelationshipsCommand.DeleteDestinationNodes)
                    {
                        await _cosmosDbService.DeleteItemAsync(databaseName, relationshipContentType!, relationshipId);
                    }
                    else
                    {
                        await _cosmosDbService.DeleteIncomingRelationshipAsync(databaseName,
                            relationshipContentType!, relationshipId, $"{contentType}{id}");
                    }
                }
            }
        }

        private static string? GetIdentifierFieldKey(Dictionary<string, object>? properties)
        {
            if (properties == null)
            {
                return null;
            }

            if (properties.ContainsKey("uri"))
            {
                return "uri";
            }

            return "userId";
        }

        private async Task MergeNodeCommand(string databaseName, ICommand command)
        {
            var properties = command.Query.Parameters["properties"] as Dictionary<string, object>;
            var identifier = GetIdentifierFieldKey(properties);

            if (identifier == null || !properties!.ContainsKey(identifier))
            {
                throw new MissingFieldException("The identifier is missing - this is needed for data synchronisation");
            }

            var itemUri = (string)properties[identifier];
            (_, Guid id) = GetContentTypeAndId(itemUri);

            var contentType = ((string)properties["ContentType"]).ToLower();

            var item = await _cosmosDbService.GetContentItemFromDatabase(databaseName, contentType, id)
                ?? new Dictionary<string, object>
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
            (_, Guid id) = GetContentTypeAndId(itemUri);
            var contentType = ((CosmosDbReplaceRelationshipsCommand)command).SourceNodeLabels.First().ToLower();

            var item = await _cosmosDbService.GetContentItemFromDatabase(databaseName, contentType, id);
            var itemRelationships = (List<CommandRelationship>)commandParameters["relationships"];

            await HandleRemovedRelationshipReferences(databaseName, item, itemRelationships);

            item ??= new Dictionary<string, object>
            {
                {"id", id},
                {"ContentType", contentType},
                {"_links", BuildLinksDictionary(itemUri)},
            };

            await HandleAddedRelationshipReferences(databaseName, itemRelationships, id, contentType, item);
            HandleInternalRelationships(itemUri, item, itemRelationships);

            await _cosmosDbService.UpdateItemAsync(databaseName, item);
        }

        private static string GetItemOrItems(int count)
        {
            return count == 1 ? "item" : "items";
        }

        private void HandleInternalRelationships(
            string itemUri,
            Dictionary<string, object> item,
            List<CommandRelationship> itemRelationships)
        {
            Dictionary<string, object> itemLinks;

            if (item.ContainsKey("_links"))
            {
                itemLinks = SafeCastToDictionary(item["_links"]);
            }
            else
            {
                itemLinks = BuildLinksDictionary(itemUri);
                item.Add("_links", itemLinks);
            }

            var relationships = itemRelationships
                .Where(relationship =>
                {
                    string href = ((string)relationship.DestinationNodeIdPropertyValues.FirstOrDefault()!).ExtactCurieHref();
                    return !string.IsNullOrEmpty(href);
                })
                .Select(ExtractRelationship)
                .ToList();

            var relationshipKeys = relationships
                .Select(relationship => relationship.Key)
                .Distinct();

            foreach (var itemToRemove in itemLinks.Where(link => link.Key.StartsWith("cont:") && !relationshipKeys.Contains(link.Key)))
            {
                itemLinks.Remove(itemToRemove.Key);
            }

            foreach (var relationshipKey in relationshipKeys)
            {
                var keyValues = relationships
                    .Where(relationship => relationship.Key == relationshipKey)
                    .Select(relationship => relationship.Relationship)
                    .ToArray();

                var itemObject = keyValues.Length == 1 ? keyValues.First() : null;
                var itemArray = keyValues.Length != 1 ? keyValues : null;

                if (!itemLinks.ContainsKey(relationshipKey))
                {
                    itemLinks.Add(relationshipKey, (object?)itemObject ?? itemArray!);
                    continue;
                }

                // Replace item with item or array
                itemLinks[relationshipKey] = (object?)itemObject ?? itemArray!;
            }

            item["_links"] = itemLinks;
        }

        private static (string Key, JObject Relationship) ExtractRelationship(CommandRelationship relationship)
        {
            string relationshipKey = $"cont:{relationship.RelationshipType}";

            var valuesDictionary = new JObject
            {
                new JProperty("href", ((string)relationship.DestinationNodeIdPropertyValues.FirstOrDefault()!).ExtactCurieHref()),
                new JProperty("contentType", relationship.DestinationNodeLabels.First(lab => lab != "Resource"))
            };

            if (relationship.Properties != null)
            {
                foreach ((string? key, object? value) in relationship.Properties)
                {
                    valuesDictionary.Add(new JProperty(key, value));
                }
            }

            return (relationshipKey, valuesDictionary);
        }

        private static string GetRelationshipId(CommandRelationship commandRelationship)
        {
            if (!(commandRelationship.DestinationNodeIdPropertyValues.FirstOrDefault() is string uri))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(uri))
            {
                return string.Empty;
            }

            (string? contentType, Guid id) = GetContentTypeAndId(uri);
            return $"{contentType}{id}";
        }

        private async Task HandleRemovedRelationshipReferences(
            string databaseName,
            Dictionary<string, object>? itemBeforeUpdate,
            List<CommandRelationship> itemRelationships)
        {
            if (itemBeforeUpdate == null) return;

            string itemId = (string)itemBeforeUpdate["ContentType"] + (string)itemBeforeUpdate["id"];

            var existingItemRelationships = SafeCastToDictionary(itemBeforeUpdate["_links"])
                .GetLinks()
                .SelectMany(l => l.Value.Select(GetContentTypeAndId));

            var newRelationshipIds = itemRelationships.Select(GetRelationshipId)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            // Filter for any existing relationships that are no longer present in the new relationships
            foreach (var item in existingItemRelationships
                .Where(i => newRelationshipIds.All(n => $"{i.ContentType}{i.Id}" != n)))
            {
                await _cosmosDbService.DeleteIncomingRelationshipAsync(databaseName, item.ContentType!, item.Id, itemId);
            }
        }

        private async Task HandleAddedRelationshipReferenceIncoming(
            CommandRelationship relationship,
            Dictionary<string, object> item,
            string relationshipUri,
            string relationshipContentType,
            string databaseName)
        {
            if (string.IsNullOrEmpty(relationship.IncomingRelationshipType) || !item.ContainsKey("_links"))
            {
                return;
            }

            var itemLinks = SafeCastToDictionary(item["_links"]);
            var key = $"cont:{relationship.IncomingRelationshipType}";
            var href = new Uri(relationshipUri).AbsolutePath.Replace("/api/execute", string.Empty);

            var itemToAdd = new Dictionary<string, object>
            {
                {"href", href},
                {"contentType", relationshipContentType},
                {"twoWay", true},
            };

            if (!itemLinks.ContainsKey(key))
            {
                itemLinks.Add(key, itemToAdd);

                item["_links"] = itemLinks;
                await _cosmosDbService.UpdateItemAsync(databaseName, item);

                return;
            }

            var existingList = CanCastToList(itemLinks[key])
                ? SafeCastToList(itemLinks[key])
                : new List<Dictionary<string, object>> {SafeCastToDictionary(itemLinks[key])};

            var anyAdditions = false;

            // Extra to list is so we can add inside the collection
            foreach (var listItem in existingList)
            {
                var existingHref = (string)listItem["href"];

                if (href != existingHref)
                {
                    anyAdditions = true;
                    break;
                }
            }

            if (anyAdditions)
            {
                existingList.Add(itemToAdd);
                itemLinks[key] = existingList;
                item["_links"] = itemLinks;

                await _cosmosDbService.UpdateItemAsync(databaseName, item);
            }
        }

        private async Task HandleAddedRelationshipReferences(
            string databaseName,
            List<CommandRelationship> itemRelationships,
            Guid itemId,
            string itemContentType,
            Dictionary<string, object> item)
        {
            foreach (var relationship in itemRelationships
                .Where(r => !string.IsNullOrEmpty(r.DestinationNodeIdPropertyName)))
            {
                string relationshipUri = (string)relationship.DestinationNodeIdPropertyValues.First();
                (string? relationshipContentType, Guid relationshipId) = GetContentTypeAndId(relationshipUri);

                await HandleAddedRelationshipReferenceIncoming(relationship, item, relationshipUri, relationshipContentType!, databaseName);

                var relationshipItem = await _cosmosDbService.GetContentItemFromDatabase(databaseName, relationshipContentType!, relationshipId);

                if (relationshipItem == null || !relationshipItem.ContainsKey("_links"))
                {
                    continue;
                }

                var links = SafeCastToDictionary(relationshipItem["_links"]);
                var curies = SafeCastToList(links["curies"]);
                int incomingPosition = curies.FindIndex(curie =>
                    (string)curie["name"] == "incoming");
                var incomingObject = curies.Count > incomingPosition ? curies[incomingPosition] : null;

                if (incomingObject == null)
                {
                    throw new MissingFieldException("Incoming collection missing");
                }

                var incomingList = SafeCastToList(incomingObject["items"]);
                bool itemLinkAlreadyExists = false;

                foreach (var incomingItem in incomingList)
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

        private async Task RemoveItemFromIncomingLinks(string databaseName, List<Dictionary<string, object>> incomingLinks, Guid itemId)
        {
            foreach (var linkedItem in incomingLinks)
            {
                string contentTypeOfLinkedItem = (string)linkedItem["contentType"];
                var linkedItemId = new Guid((string)linkedItem["id"]);
                var linkedItemFromCosmos = await _cosmosDbService.GetContentItemFromDatabase(databaseName, contentTypeOfLinkedItem, linkedItemId);

                if (linkedItemFromCosmos == null)
                {
                    throw new ValidationException("Failed to delete Content Item. Linked items from cosmos returned NULL.");
                }

                Dictionary<string, object> linkedItemLinks = SafeCastToDictionary(linkedItemFromCosmos["_links"]);
                var keysInLinkedItem = linkedItemLinks.Keys.Where(key => key.Contains("cont:")).ToList();

                if (keysInLinkedItem == null)
                {
                    throw new ValidationException("Failed to delete Content Item. Keys in linked items returned NULL.");
                }

                foreach (var key in keysInLinkedItem)
                {
                    if (CanCastToList(linkedItemLinks[key]))
                    {
                        List<Dictionary<string, object>>  listOfItemsInContKey = SafeCastToList(linkedItemLinks[key]);
                        var itemToRemove = listOfItemsInContKey.FirstOrDefault(dict => dict.ContainsKey("href") && (dict["href"].ToString() ?? "").Contains(itemId.ToString()));

                        if (itemToRemove != null)
                        {
                            listOfItemsInContKey.Remove(itemToRemove);
                            linkedItemLinks[key] = listOfItemsInContKey;
                            break;
                        }

                    }
                    else
                    {
                        Dictionary<string, object> itemInContKey = SafeCastToDictionary(linkedItemLinks[key]);
                        var isIdInCurrentKey = (itemInContKey["href"].ToString() ?? "").Contains(itemId.ToString());

                        if (isIdInCurrentKey)
                        {
                            linkedItemLinks.Remove(key);
                            break;
                        }
                    }
                }

                linkedItemFromCosmos["_links"] = linkedItemLinks;
                await _cosmosDbService.UpdateItemAsync(databaseName, linkedItemFromCosmos);
            }
        }

        private static Dictionary<string, object> BuildLinksDictionary(string uri)
        {
            string[] uriParts = uri.Split('/');
            var contLink = uri;

            if (uriParts.Length >= 5)
            {
                contLink = $"https://{uriParts[2]}/{uriParts[3]}/{uriParts[4]}";
            }

            return new Dictionary<string, object>
            {
                { "self", uri },
                { "curies", new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "name", "cont" },
                            { "href", contLink }
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
