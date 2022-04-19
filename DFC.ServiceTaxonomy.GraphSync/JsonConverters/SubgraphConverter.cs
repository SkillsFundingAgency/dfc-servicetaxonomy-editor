using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using static DFC.ServiceTaxonomy.GraphSync.Helpers.DocumentHelper;
using static DFC.ServiceTaxonomy.GraphSync.Helpers.UniqueNumberHelper;

namespace DFC.ServiceTaxonomy.GraphSync.JsonConverters
{
    public class SubgraphConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var data = JObject.Load(reader);
            var links = SafeCastToDictionary(data["_links"]);

            string dataId = GetAsString(data["id"]!);
            string dataContentType = (string)data["ContentType"]!;

            int endNodeId = GetNumber(dataId);

            var incomingRelationships = new List<IRelationship>();
            var nodes = new List<INode>();

            foreach (var incomingItem in GetIncomingList(links))
            {
                string incomingItemContentType = (string)incomingItem["contentType"];
                string incomingItemId = GetAsString(incomingItem["id"]);

                int startNodeId = GetNumber(incomingItemId);
                int relationshipId = GetNumber(GetAsString(dataId) + incomingItemId);

                incomingRelationships.Add(new StandardRelationship
                {
                    Type = $"has{FirstCharToUpper(dataContentType)}",
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

            return new Subgraph(
                nodes,
                incomingRelationships,
                new StandardNode
                {
                    Labels = new List<string> { dataContentType, "Resource" },
                    Properties = new Dictionary<string, object>
                    {
                        { "ContentType", dataContentType },
                        { "id", dataId },
                    },
                    Id = endNodeId
                });
        }

        private List<Dictionary<string, object>> GetIncomingList(Dictionary<string, object> links)
        {
            var curies = SafeCastToList(
                links.SingleOrDefault(link => link.Key == "curies").Value);

            int incomingPosition = curies!.FindIndex(curie =>
                (string)curie["name"] == "incoming");

            var incomingObject = curies.Count > incomingPosition ? curies[incomingPosition] : null;

            if (incomingObject == null)
            {
                throw new MissingFieldException("Incoming property missing");
            }

            return SafeCastToList(incomingObject["items"]);
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
