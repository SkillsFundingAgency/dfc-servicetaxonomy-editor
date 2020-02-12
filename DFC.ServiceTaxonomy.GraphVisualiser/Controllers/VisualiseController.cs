using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

// https://localhost:5001/index.html?visualise=theid

namespace DFC.ServiceTaxonomy.GraphVisualiser.Controllers
{
//     public class Neo4jResponse
//     {
//         public string[] Keys { get; set; }
//         //public int Length { get; set; }
//         public Neo4jObject[] _Fields { get; set; }
//         //_fieldLookup
//     }
//
//     public class Neo4jObject
//     {
//         public ulong Id { get; set; }
//
//         // public static Neo4jObject Create()
//         // {
//         //     if (labels != null)
//         //         return
//         // }
//     }
//
//     public class Node : Neo4jObject
//     {
//         public string[] Labels { get; set; }
//         public Dictionary<string, object> Properties { get; set; }
//     }
//
//     public class Relationship : Neo4jObject
//     {
//         public ulong StartNodeId { get; set; }
//         public ulong EndNodeId { get; set; }
//         public string Type { get; set; }
//         public Dictionary<string, object> Properties { get; set; }
//     }
//
//     public class Identity
//     {
//         public uint Low { get; set; }
//         public uint High { get; set; }
//     }
//
//     public class Neo4jObjectJsonConverter : JsonConverter<Neo4jObject>
// {
//     public override bool CanConvert(Type type)
//     {
//         return typeof(Neo4jObject).IsAssignableFrom(type);
//     }
//
//     public override Neo4jObject Read(
//         ref Utf8JsonReader reader,
//         Type typeToConvert,
//         JsonSerializerOptions options)
//     {
//         if (reader.TokenType != JsonTokenType.StartObject)
//             throw new JsonException();
//
//         Identity identity = null;
//         string[] labels = null;
//         string type = null;
//
//         while (reader.Read())
//         {
//             switch (reader.TokenType)
//             {
//                 case JsonTokenType.PropertyName:
//                     string propertyName = reader.GetString();
//                     switch (propertyName)
//                     {
//                         case "identity":
//                             identity = JsonSerializer.Deserialize<Identity>(ref reader);
//                             break;
//                         case "labels":
//                             labels = JsonSerializer.Deserialize<string[]>(ref reader);
//                             break;
//                         case "type":
//                             reader.Read();
//                             type = reader.GetString();
//                             break;
//                         default:
//                             throw new NotSupportedException();
//                     }
//                     break;
//             }
//         }
//
//         //todo: identity combine into uint
//         if (labels != null)
//             return new Node
//             {
//                 Id = identity?.Low ?? throw new JsonException(),
//                 Labels = labels
//             };
//         if (type != null)
//             return new Relationship
//             {
//                 Id = identity?.Low ?? throw new JsonException(),
//                 Type = type
//             };
//
//         throw new JsonException();
//
//         // if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
//         // {
//         //     throw new JsonException();
//         // }
//     }
//
//     public override void Write(
//         Utf8JsonWriter writer,
//         Neo4jObject value,
//         JsonSerializerOptions options)
//     {
//         writer.WriteStartObject();
//
//         switch (value)
//         {
//             case Node node:
//                 JsonSerializer.Serialize(writer, node);
//                 break;
//             case Relationship relationship:
//                 JsonSerializer.Serialize(writer, relationship);
//                 break;
//             default:
//                 throw new NotSupportedException();
//         }
//
//         writer.WriteEndObject();
//     }
// }

    //todo: wwwroot files need to be in this module
    public class VisualiseController : Controller
    {
        private readonly IGraphDatabase _neoGraphDatabase;

        public VisualiseController(IGraphDatabase neoGraphDatabase)
        {
            _neoGraphDatabase = neoGraphDatabase ?? throw new ArgumentNullException(nameof(neoGraphDatabase));
        }

        public async Task<ActionResult> Data([FromQuery] string fetch, [FromQuery] string id)
        {
            //Neo4j.Driver.Internal.Types.Node
            var nodes = new Dictionary<long,INode>();
            var relationships = new HashSet<IRelationship>();
            fetch = "http://nationalcareers.service.gov.uk/jobprofile/c07791e0-9e78-480f-b4ac-db39e4582496";
            var results = await _neoGraphDatabase.RunReadQuery(
                new Query(
                    $"match (n:ncs__JobProfile {{uri:\"{fetch}\"}})-[r]-(d) return n, d, r"),
                r =>
                {
                    var sourceNode = r["n"].As<INode>();
                    var destNode = r["d"].As<INode>();
                    relationships.Add(r["r"].As<IRelationship>());

                    nodes[sourceNode.Id] = sourceNode;
                    nodes[destNode.Id] = destNode;

                    //todo:
                    return 0;
                });

            var response = new StringBuilder(@"{
   ""_comment"": ""Empty ontology for WebVOWL Editor [Additional Information added by WebVOWL Exporter Version: 1.1.7]"",
   ""header"": {
     ""languages"": [
       ""en""
     ],
     ""baseIris"": [
       ""http://www.w3.org/2000/01/rdf-schema""
     ],
     ""iri"": ""https://nationalcareers.service.gov.uk/test"",
     ""title"": ""test"",
     ""description"": {
       ""en"": ""New ontology description""
     }
   },
   ""namespace"": [],
   ""settings"": {
     ""global"": {
       ""zoom"": ""2.09"",
       ""translation"": [
         -1087.15,
         -750.73
       ],
       ""paused"": false
     },
     ""gravity"": {
       ""classDistance"": 200,
       ""datatypeDistance"": 120
     },
     ""filter"": {
       ""checkBox"": [
         {
           ""id"": ""datatypeFilterCheckbox"",
           ""checked"": false
         },
         {
           ""id"": ""objectPropertyFilterCheckbox"",
           ""checked"": false
         },
         {
           ""id"": ""subclassFilterCheckbox"",
           ""checked"": false
         },
         {
           ""id"": ""disjointFilterCheckbox"",
           ""checked"": true
         },
         {
           ""id"": ""setoperatorFilterCheckbox"",
           ""checked"": false
         }
       ],
       ""degreeSliderValue"": ""0""
     },
     ""modes"": {
       ""checkBox"": [
         {
           ""id"": ""pickandpinModuleCheckbox"",
           ""checked"": false
         },
         {
           ""id"": ""nodescalingModuleCheckbox"",
           ""checked"": true
         },
         {
           ""id"": ""compactnotationModuleCheckbox"",
           ""checked"": false
         },
         {
           ""id"": ""colorexternalsModuleCheckbox"",
           ""checked"": true
         }
       ],
       ""colorSwitchState"": false
     }
   },
   ""class"": [");

            // {
//       ""id"": ""Class1"",
//       ""type"": ""owl:Class""
//     }

            response.AppendJoin(',', nodes.Select(n => $"{{ \"id\": \"{n.Key}\", \"type\": \"own:Class\" }}"));

            response.Append("], \"classAttribute\": [");

            //     {
//       ""id"": ""Class1"",
//       ""iri"": ""https://nationalcareers.service.gov.uk/testJobProfile"",
//       ""baseIri"": ""http://visualdataweb.org/newOntology/"",
//       ""label"": ""JobProfile"",
//         ""pos"": [
//             864.79,
//             688.54
//         ]
//     },

// todo: center current content item?
// mark node label to display in graph??

            response.AppendJoin(',', nodes.Select(n => $@"{{
       ""id"": ""{n.Key}"",
       ""iri"": ""https://nationalcareers.service.gov.uk/testJobProfile"",
       ""baseIri"": ""http://visualdataweb.org/newOntology/"",
       ""label"": ""{n.Value.Labels.First(l => l != "Resource")}""}}"));

            response.Append("],\"property\": [");

            // {
            //     ""id"": ""objectProperty1"",
            //     ""type"": ""owl:ObjectProperty""
            // }

            response.AppendJoin(',', relationships.Select(r => $@"{{
            ""id"": ""{r.Id}"",
            ""type"": ""owl:ObjectProperty""}}"));

            response.Append("], \"propertyAttribute\": [");

            //     {
//       ""id"": ""objectProperty1"",
//       ""iri"": ""https://nationalcareers.service.gov.uk/testhasOccupation"",
//       ""baseIri"": ""https://nationalcareers.service.gov.uk/test"",
//       ""label"": ""hasOccupation"",
//       ""attributes"": [
//         ""object""
//       ],
//       ""domain"": ""Class1"",
//       ""range"": ""Class2""
//     }

            response.AppendJoin(',', relationships.Select(r => $@"{{
       ""id"": ""{r.Id}"",
       ""iri"": ""https://nationalcareers.service.gov.uk/testhasOccupation"",
       ""baseIri"": ""https://nationalcareers.service.gov.uk/test"",
       ""label"": ""{r.Type}"",
       ""attributes"": [
         ""object""
       ],
       ""domain"": ""{r.StartNodeId}"",
       ""range"": ""{r.EndNodeId}""
}}"));

            response.Append("]}");

            return Content(response.ToString(), "application/json");
        }

//         public ActionResult Data([FromQuery]string fetch)
//         {
//             //return Json();
//             return Content(@"{
//   ""_comment"": ""Empty ontology for WebVOWL Editor [Additional Information added by WebVOWL Exporter Version: 1.1.7]"",
//   ""header"": {
//     ""languages"": [
//       ""en""
//     ],
//     ""baseIris"": [
//       ""http://www.w3.org/2000/01/rdf-schema""
//     ],
//     ""iri"": ""https://nationalcareers.service.gov.uk/test"",
//     ""title"": ""test"",
//     ""description"": {
//       ""en"": ""New ontology description""
//     }
//   },
//   ""namespace"": [],
//   ""settings"": {
//     ""global"": {
//       ""zoom"": ""2.09"",
//       ""translation"": [
//         -1087.15,
//         -750.73
//       ],
//       ""paused"": false
//     },
//     ""gravity"": {
//       ""classDistance"": 200,
//       ""datatypeDistance"": 120
//     },
//     ""filter"": {
//       ""checkBox"": [
//         {
//           ""id"": ""datatypeFilterCheckbox"",
//           ""checked"": false
//         },
//         {
//           ""id"": ""objectPropertyFilterCheckbox"",
//           ""checked"": false
//         },
//         {
//           ""id"": ""subclassFilterCheckbox"",
//           ""checked"": false
//         },
//         {
//           ""id"": ""disjointFilterCheckbox"",
//           ""checked"": true
//         },
//         {
//           ""id"": ""setoperatorFilterCheckbox"",
//           ""checked"": false
//         }
//       ],
//       ""degreeSliderValue"": ""0""
//     },
//     ""modes"": {
//       ""checkBox"": [
//         {
//           ""id"": ""pickandpinModuleCheckbox"",
//           ""checked"": false
//         },
//         {
//           ""id"": ""nodescalingModuleCheckbox"",
//           ""checked"": true
//         },
//         {
//           ""id"": ""compactnotationModuleCheckbox"",
//           ""checked"": false
//         },
//         {
//           ""id"": ""colorexternalsModuleCheckbox"",
//           ""checked"": true
//         }
//       ],
//       ""colorSwitchState"": false
//     }
//   },
//   ""class"": [
//     {
//       ""id"": ""Class1"",
//       ""type"": ""owl:Class""
//     },
//     {
//       ""id"": ""Class2"",
//       ""type"": ""owl:Class""
//     }
//   ],
//   ""classAttribute"": [
//     {
//       ""id"": ""Class1"",
//       ""iri"": ""https://nationalcareers.service.gov.uk/testJobProfile"",
//       ""baseIri"": ""http://visualdataweb.org/newOntology/"",
//       ""label"": ""JobProfile"",
//         ""pos"": [
//             864.79,
//             688.54
//         ]
//     },
//     {
//       ""id"": ""Class2"",
//       ""iri"": ""https://nationalcareers.service.gov.uk/testOccupation"",
//       ""baseIri"": ""https://nationalcareers.service.gov.uk/test"",
//       ""label"": ""Occupation""
//     }
//   ],
//   ""property"": [
//     {
//       ""id"": ""objectProperty1"",
//       ""type"": ""owl:ObjectProperty""
//     }
//   ],
//   ""propertyAttribute"": [
//     {
//       ""id"": ""objectProperty1"",
//       ""iri"": ""https://nationalcareers.service.gov.uk/testhasOccupation"",
//       ""baseIri"": ""https://nationalcareers.service.gov.uk/test"",
//       ""label"": ""hasOccupation"",
//       ""attributes"": [
//         ""object""
//       ],
//       ""domain"": ""Class1"",
//       ""range"": ""Class2""
//     }
//   ]
// }
// ", "application/json");        }
     }
}
