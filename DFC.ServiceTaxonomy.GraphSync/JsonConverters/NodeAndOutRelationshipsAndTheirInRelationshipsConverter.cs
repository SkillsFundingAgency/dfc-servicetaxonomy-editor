using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Helpers;

namespace DFC.ServiceTaxonomy.GraphSync.JsonConverters
{
    public class NodeAndOutRelationshipsAndTheirInRelationshipsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var data = JObject.Load(reader);
            string contentType = (string)data["ContentType"]!;

            var properties = data
                .ToObject<Dictionary<string, object>>()!
                .Where(dictionary => !dictionary.Key.StartsWith("_"))
                .ToDictionary(kvp => kvp.Key, x => x.Value);

            string itemId = DocumentHelper.GetAsString(properties["id"]);
            int startNodeId = UniqueNumberHelper.GetNumber(itemId);

            var relationships = new List<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)>();
            var links = data["_links"]?.ToObject<Dictionary<string, object>>();

            var node = new StandardNode
            {
                Labels = new List<string> { contentType, "Resource" },
                Properties = properties,
                Id = startNodeId
            };

            if (links == null)
            {
                return new NodeAndOutRelationshipsAndTheirInRelationships(node, relationships);
            }

            foreach (var link in links.Where(x => x.Key != "self" && x.Key != "curies"))
            {
                var linkDictionary = (link.Value as JObject)!.ToObject<Dictionary<string, object>>();
                var linkDetails = DocumentHelper.GetContentTypeAndId((string)linkDictionary!["href"]);

                int endNodeId = UniqueNumberHelper.GetNumber(DocumentHelper.GetAsString(linkDetails.Id));
                int relationshipId = UniqueNumberHelper.GetNumber(
                    DocumentHelper.GetAsString(linkDetails.Id) + DocumentHelper.GetAsString(itemId));

                var outgoing = new OutgoingRelationship(new StandardRelationship
                {
                    Type = link.Key.Replace("cont:", string.Empty), // e.g. hasPageLocation
                    StartNodeId = startNodeId,
                    EndNodeId = endNodeId,
                    Id = relationshipId
                }, new StandardNode
                {
                    Id = endNodeId,
                    Properties = new Dictionary<string, object>
                    {
                        { "ContentType", linkDetails.ContentType },
                        { "id", linkDetails.Id },
                        { "endNodeId", endNodeId }
                    },
                    Labels = new List<string> { linkDetails.ContentType, "Resource" }
                });

                relationships.Add((outgoing, new List<OutgoingRelationship>()));
            }

            return new NodeAndOutRelationshipsAndTheirInRelationships(node, relationships);
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

    }
}
