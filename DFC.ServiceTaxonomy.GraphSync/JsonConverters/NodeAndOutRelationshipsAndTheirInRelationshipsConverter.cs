﻿using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using static DFC.ServiceTaxonomy.GraphSync.Helpers.DocumentHelper;
using static DFC.ServiceTaxonomy.GraphSync.Helpers.UniqueNumberHelper;

namespace DFC.ServiceTaxonomy.GraphSync.JsonConverters
{
    public class NodeAndOutRelationshipsAndTheirInRelationshipsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var data = JObject.Load(reader);

            var properties = data
                .ToObject<Dictionary<string, object>>()!
                .Where(dictionary => !dictionary.Key.StartsWith("_"))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            string itemId = GetAsString(properties["id"]);
            int startNodeId = GetNumber(itemId);

            var relationships = new List<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)>();
            var links = SafeCastToDictionary(data["_links"]);

            foreach (var link in links.Where(lnk => lnk.Key != "self" && lnk.Key != "curies"))
            {
                List<Dictionary<string, object>> linkDictionaries = CanCastToList(link.Value) ?
                    SafeCastToList(link.Value) :
                    new List<Dictionary<string, object>> { SafeCastToDictionary(link.Value) };

                foreach (var linkDictionary in linkDictionaries)
                {
                    (_, Guid id) = GetContentTypeAndId((string)linkDictionary["href"]);

                    if (id == Guid.Empty)
                    {
                        continue;
                    }

                    int endNodeId = GetNumber(GetAsString(id));
                    int relationshipId = GetNumber(
                        GetAsString(id) + GetAsString(itemId));
                    var itemContentType = (string)linkDictionary["contentType"];

                    var outgoing = new OutgoingRelationship(new StandardRelationship
                    {
                        Type = link.Key.Replace("cont:", string.Empty), // e.g. hasPageLocation
                        StartNodeId = startNodeId,
                        EndNodeId = endNodeId,
                        Id = relationshipId
                    },
                    new StandardNode
                    {
                        Id = endNodeId,
                        Properties = new Dictionary<string, object>
                        {
                            {"ContentType", itemContentType},
                            {"id", id},
                            {"endNodeId", endNodeId}
                        },
                        Labels = new List<string> { itemContentType, "Resource" }
                    });

                    relationships.Add((outgoing, new List<OutgoingRelationship>()));
                }
            }

            string contentType = (string)data["ContentType"]!;

            var node = new StandardNode
            {
                Labels = new List<string> { contentType, "Resource" },
                Properties = properties,
                Id = startNodeId
            };

            return new NodeAndOutRelationshipsAndTheirInRelationships(node, relationships);
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
