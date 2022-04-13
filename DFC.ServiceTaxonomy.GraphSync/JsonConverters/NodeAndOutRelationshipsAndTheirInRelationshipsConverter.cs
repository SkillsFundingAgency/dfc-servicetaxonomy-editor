using System;
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
            string contentType = (string)data["ContentType"]!;

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

                var linkDictionarys = SafeObjectArrayCastToDictionaryList(link.Value);
                foreach (Dictionary<string, object> linkDictionary in linkDictionarys)
                {
                    var linkDetails = GetContentTypeAndId((string)linkDictionary!["href"]);
                    if (linkDetails.Id == Guid.Empty)
                    {
                        continue;
                    }
                    int endNodeId = GetNumber(GetAsString(linkDetails.Id));
                    int relationshipId = GetNumber(
                        GetAsString(linkDetails.Id) + GetAsString(itemId));

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
            }

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
