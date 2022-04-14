using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static DFC.ServiceTaxonomy.GraphSync.Helpers.DocumentHelper;

namespace DFC.ServiceTaxonomy.GraphSync.JsonConverters
{
    public class NodeWithOutgoingRelationshipsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var contentType = (string)jobj["ContentType"] ?? "unknown";
            var parentId = GetAsString(jobj["id"]!);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            var startNodeId = UniqueNumberHelper.GetNumber(parentId);

            var properties = jobj
                .ToObject<Dictionary<string, object>>()!
                .Where(x => !x.Key.StartsWith("_"))
                .ToDictionary(x => x.Key, x => x.Value);

            var relationships = new List<(IRelationship relationship, INode destinationNode)>();
            var links = jobj["_links"]?.ToObject<Dictionary<string, object>>();

            if (links != null)
            {
                foreach ((string? key, object? value) in links.Where(x => x.Key != "self" && x.Key != "curies"))
                {
                    Dictionary<string, object> linkDictionary = SafeCastToDictionary(value);
                    string linkHref = (string)linkDictionary["href"];
                    (string linkContentType, Guid linkId) = GetContentTypeAndId(linkHref);

                    int endNodeId = UniqueNumberHelper.GetNumber(GetAsString(linkId));

                    var relationship = new StandardRelationship
                    {
                        Type = key.Replace("cont:", string.Empty),
                        StartNodeId = startNodeId,
                        EndNodeId = endNodeId,
                        Id = UniqueNumberHelper.GetNumber(GetAsString(linkId) + GetAsString(parentId))
                    };

                    var relationshipNode = new StandardNode
                    {
                        Id = endNodeId,
                        Labels = new List<string> { linkContentType, "Resource" },
                        Properties = new Dictionary<string, object> {{ "uri", linkHref }, { "contentType", linkContentType }}
                    };

                    relationships.Add((relationship, relationshipNode));
                }
            }

            var parentNode = new StandardNode
            {
                Labels = new List<string> { contentType, "Resource" },
                Properties = properties
            };

            return new CosmosDbNodeWithOutgoingRelationships(parentNode, relationships);
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
