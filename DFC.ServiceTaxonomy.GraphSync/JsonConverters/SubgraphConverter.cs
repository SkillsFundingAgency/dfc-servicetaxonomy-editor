using System;
using System.Collections.Generic;
using System.Globalization;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Helper;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.JsonConverters
{
    public class SubgraphConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var data = JObject.Load(reader);
            var links = data["_links"]?.ToObject<Dictionary<string, object>>();

            if (links == null)
            {
                return new Subgraph(new List<INode>(), new List<IRelationship>(), null);
            }

            var dataId = GetAsString(data["id"]!);
            var dataContentType = (string)data["ContentType"]!;
            var endNodeId = UniqueSequencedNumber.GetNumber(dataId);
            var incomingRelationships = new List<IRelationship>();
            var nodes = new List<INode>();

            var curies = (links.SingleOrDefault(x => x.Key == "curies").Value as JArray)!
                .ToObject<List<Dictionary<string, object>>>();
            var incomingPosition = curies!.FindIndex(curie =>
                (string)curie["name"] == "incoming");

            var incomingObject = curies.Count > incomingPosition ? curies[incomingPosition] : null;

            if (incomingObject == null)
            {
                throw new MissingFieldException("Incoming property missing");
            }

            var incomingList = (incomingObject["items"] as JArray)!.ToObject<List<Dictionary<string, object>>>();

            foreach (var incomingItem in incomingList!)
            {
                var contentType = (string)incomingItem["contentType"];
                var id = GetAsString(incomingItem["id"]);

                var startNodeId = UniqueSequencedNumber.GetNumber(id);
                var relationshipId = UniqueSequencedNumber.GetNumber(GetAsString(dataId) + id);

                incomingRelationships.Add(new StandardRelationship
                {
                    Type = $"has{FirstCharToUpper(dataContentType)}",
                    StartNodeId = startNodeId,
                    EndNodeId = endNodeId,
                    Id = relationshipId
                });

                nodes.Add(new StandardNode
                {
                    Labels = new List<string> { contentType, "Resource" },
                    Properties = new Dictionary<string, object>
                    {
                        { "ContentType", contentType },
                        { "id", id },
                    },
                    Id = startNodeId
                });
            }

            if (!nodes.Any() && !incomingRelationships.Any())
            {
                return null;
            }

            return new Subgraph(nodes, incomingRelationships);
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

        private static string FirstCharToUpper(string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
