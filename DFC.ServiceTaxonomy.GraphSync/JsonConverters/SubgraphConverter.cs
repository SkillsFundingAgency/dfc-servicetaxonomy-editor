using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Helpers;
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

            int idAsNumber = GetNumber(dataId);

            var relationships = new List<IRelationship>();
            var nodes = new List<INode>();

            if (links == null)
            {
                throw new MissingFieldException("Links property is missing");
            }

            foreach (var link in links.Where(lnk => lnk.Key != "self" && lnk.Key != "curies"))
            {
                List<Dictionary<string, object>> linkDictionaries = CanCastToList(link.Value) ?
                    SafeCastToList(link.Value) :
                    new List<Dictionary<string, object>> { SafeCastToDictionary(link.Value) };

                foreach (var linkDictionary in linkDictionaries)
                {
                    string href = GetAsString(linkDictionary["href"]);
                    var (contentType, id) = GetContentTypeAndId(href);

                    int endNodeId = GetNumber(id.ToString());
                    int relationshipId = GetNumber(GetAsString(dataId) + id);

                    relationships.Add(new StandardRelationship
                    {
                        Type = link.Key.Replace("cont:", string.Empty),
                        StartNodeId = idAsNumber,
                        EndNodeId = endNodeId,
                        Id = relationshipId,
                        Properties = linkDictionary
                            .Where(pair => pair.Key != "href")
                            .ToDictionary(pair => pair.Key, pair => pair.Value)
                    });

                    nodes.Add(new StandardNode
                    {
                        Labels = new List<string> { contentType, "Resource" },
                        Properties = new Dictionary<string, object>
                        {
                            { "ContentType", contentType },
                            { "id", id },
                            { "uri", href }
                        },
                        Id = endNodeId
                    });
                }
            }

            foreach (var incomingItem in GetIncomingList(links))
            {
                string incomingItemContentType = (string)incomingItem["contentType"];
                string incomingItemId = GetAsString(incomingItem["id"]);

                int startNodeId = GetNumber(incomingItemId);
                int relationshipId = GetNumber(GetAsString(dataId) + incomingItemId);

                relationships.Add(new StandardRelationship
                {
                    Type = $"has{FirstCharToUpper(dataContentType)}",
                    StartNodeId = startNodeId,
                    EndNodeId = idAsNumber,
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

            if (!nodes.Any() && !relationships.Any())
            {
                return null;
            }

            return new Subgraph(
                nodes,
                relationships,
                new StandardNode
                {
                    Labels = new List<string> { dataContentType, "Resource" },
                    Properties = GetProperties(data),
                    Id = idAsNumber
                });
        }

        private Dictionary<string, object> GetProperties(JObject data)
        {
            var properties = new Dictionary<string, object>();
            foreach (var property in data)
            {
                if (DocumentHelper.CosmosPropsToIgnore.Contains(property.Key))
                {
                    continue;
                }

                properties.Add(property.Key, property.Value?.ToString()!);
            }

            return properties;
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
