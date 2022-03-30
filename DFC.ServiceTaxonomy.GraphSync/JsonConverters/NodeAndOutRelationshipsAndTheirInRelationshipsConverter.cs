using System;
using System.Collections.Generic;
using System.Globalization;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Helper;

namespace DFC.ServiceTaxonomy.GraphSync.JsonConverters
{
    public class NodeAndOutRelationshipsAndTheirInRelationshipsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var data = JObject.Load(reader);

            #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var contentType = (string)data["ContentType"] ?? "unknown";
            #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            var properties = data
                .ToObject<Dictionary<string, object>>()!
                .Where(dictionary => !dictionary.Key.StartsWith("_"))
                .ToDictionary(kvp => kvp.Key, x => x.Value);

            var id = GetAsString(properties["id"]);
            var startNodeId = UniqueSequencedNumber.GetNumber(id);

            var relationships = new List<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)>();
            var links = data["_links"]?.ToObject<Dictionary<string, object>>();

            if (links != null)
            {
                foreach (var link in links.Where(x => x.Key != "self" && x.Key != "curies"))
                {
                    var linkDictionary = (link.Value as JObject)!.ToObject<Dictionary<string, object>>();
                    var linkDetails = GetContentTypeAndId((string)linkDictionary!["href"]);

                    var endNodeId = UniqueSequencedNumber.GetNumber(GetAsString(linkDetails.Id));
                    var relationshipId = UniqueSequencedNumber.GetNumber(GetAsString(linkDetails.Id) + GetAsString(id));

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

        protected static (string ContentType, Guid Id) GetContentTypeAndId(string uri)
        {
            var pathOnly = uri.StartsWith("http") ? new Uri(uri, UriKind.Absolute).AbsolutePath : uri;
            pathOnly = pathOnly.ToLower().Replace("/api/execute", string.Empty);

            var uriParts = pathOnly.Trim('/').Split('/');
            var contentType = uriParts[0].ToLower();
            var id = Guid.Parse(uriParts[1]);

            return (contentType, id);
        }

        private static string GetAsString(object item)
        {
            if (item is Guid guidItem)
            {
                return guidItem.ToString();
            }

            if (item is JValue jValueItem)
            {
                return jValueItem.ToString(CultureInfo.InvariantCulture);
            }

            return (string)item;
        }
    }
}
