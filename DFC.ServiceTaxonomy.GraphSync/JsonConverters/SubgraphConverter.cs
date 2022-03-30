using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Helpers;
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
                return new Subgraph(new List<INode>(), new List<IRelationship>());
            }

            string dataId = DocumentHelper.GetAsString(data["id"]!);
            string dataContentType = (string)data["ContentType"]!;

            int endNodeId = UniqueNumberHelper.GetNumber(dataId);

            var incomingRelationships = new List<IRelationship>();
            var nodes = new List<INode>();

            foreach (var incomingItem in GetIncomingList(links))
            {
                string incomingItemContentType = (string)incomingItem["contentType"];
                string incomingItemId = DocumentHelper.GetAsString(incomingItem["id"]);

                int startNodeId = UniqueNumberHelper.GetNumber(incomingItemId);
                int relationshipId = UniqueNumberHelper.GetNumber(DocumentHelper.GetAsString(dataId) + incomingItemId);

                incomingRelationships.Add(new StandardRelationship
                {
                    Type = $"has{DocumentHelper.FirstCharToUpper(dataContentType)}",
                    StartNodeId = startNodeId,
                    EndNodeId = endNodeId,
                    Id = relationshipId
                });

                nodes.Add(new StandardNode
                {
                    Labels = new List<string> { incomingItemContentType, "Resource" },
                    Properties = new Dictionary<string, object>
                    {
                        { "ContentType", incomingItemContentType },
                        { "id", incomingItemId },
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

        private List<Dictionary<string, object>> GetIncomingList(Dictionary<string, object> links)
        {
            var curies = (links.SingleOrDefault(x => x.Key == "curies").Value as JArray)!
                .ToObject<List<Dictionary<string, object>>>();
            int incomingPosition = curies!.FindIndex(curie =>
                (string)curie["name"] == "incoming");

            var incomingObject = curies.Count > incomingPosition ? curies[incomingPosition] : null;

            if (incomingObject == null)
            {
                throw new MissingFieldException("Incoming property missing");
            }

            return (incomingObject["items"] as JArray)!.ToObject<List<Dictionary<string, object>>>()!;
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
