using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphVisualiser.Models;
using DFC.ServiceTaxonomy.GraphVisualiser.Queries;
using DFC.ServiceTaxonomy.GraphVisualiser.Services;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement.Metadata;

// https://localhost:5001/index.html?visualise=theid

//todo:
// add a custom part that selects the visualisation rule/heuristic
// e.g. visualizing a job profile, might want to follow all relationship paths until reach another job profile (follow to self)
// in addition would want to whitelist any esco namespace nodes to occupation and skill (would you want to show skills off occupation when displaying job profile?)
// have it configurable which namespaces to white/black list & specific labels in either list to white black list?
// gonna get quite complicated!
// e.g. for tasks, would probably only want to visualise the task and first degree relationships
// that allows different behaviour for different types, but in a generic manner
// we can set up the existing types sensibly, but any added types the user can set themeselves

// ^^ instead, add property to bagitem created relationships to say bag or embedded or similar
// then follow to 1 degree where relationship doesn't have embedded property, with special handling for esco?

//todo: (a) either group related nodes of same type together (like neo's browser)
// or (b) have combined data/schema visualiser

// (a) atm just overriding colour, so layout engine doesn't know anything about type
// add property to classattribute of type and change renderer
// or check out all existing owl types and see if can piggy-back or build on any

// (b) e.g. (software developer:jobProfile)--(Tasks)--(coding)

//todo: move visualisation into GraphSync module? then could add view graph button to sync part (along with id)
//todo: on properties pop.up have edit button which links to content edit page

//todo: maxLabelWidth to 180 : need to add options support to import json

namespace DFC.ServiceTaxonomy.GraphVisualiser.Controllers
{
    public class VisualiseController : Controller
    {
        private readonly IGraphDatabase _neoGraphDatabase;
        private readonly IContentDefinitionManager ContentDefinitionManager;
        private readonly INeo4JToOwlGeneratorService Neo4JToOwlGeneratorService;
        private readonly IOrchardToOwlGeneratorService OrchardToOwlGeneratorService;

        private readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public VisualiseController(IGraphDatabase neoGraphDatabase, IContentDefinitionManager contentDefinitionManager, INeo4JToOwlGeneratorService neo4jToOwlGeneratorService, IOrchardToOwlGeneratorService orchardToOwlGeneratorService)
        {
            _neoGraphDatabase = neoGraphDatabase ?? throw new ArgumentNullException(nameof(neoGraphDatabase));
            ContentDefinitionManager = contentDefinitionManager ?? throw new ArgumentNullException(nameof(contentDefinitionManager));
            Neo4JToOwlGeneratorService = neo4jToOwlGeneratorService ?? throw new ArgumentNullException(nameof(neo4jToOwlGeneratorService));
            OrchardToOwlGeneratorService = orchardToOwlGeneratorService ?? throw new ArgumentNullException(nameof(orchardToOwlGeneratorService));
        }

        public ActionResult Viewer()
        {
            return View();
        }

        public async Task<ActionResult> Data([FromQuery] string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return await GetOntology();
            }
            else
            {
                return await GetData(uri);
            }
        }

        public async Task<ActionResult> GetOntology()
        {
            var contentTypeDefinitions = ContentDefinitionManager.ListTypeDefinitions();

            var owlDataModel = OrchardToOwlGeneratorService.CreateOwlDataModels(contentTypeDefinitions);
            var owlResponseString = JsonSerializer.Serialize(owlDataModel, JsonOptions);

            return Content(owlResponseString, MediaTypeNames.Application.Json);
        }

        public async Task<ActionResult> GetData(string uri)
        {
            const string prefLabel = "skos__prefLabel";
            var query = new GetNodesCypherQuery(nameof(uri), uri, prefLabel, prefLabel);

            _ = await _neoGraphDatabase.RunReadQuery(query);

            var owlDataModel = Neo4JToOwlGeneratorService.CreateOwlDataModels(query.SelectedNodeId, query.Nodes, query.Relationships, prefLabel);
            var owlResponseString = JsonSerializer.Serialize(owlDataModel, JsonOptions);

            //TODO: Ian - Delete the following 4 lines once finished refactoring
            var originalResponseString = await OriginalData(uri);
            var xx = originalResponseString.Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty).Replace(" ", string.Empty);
            var yy = owlResponseString.Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty).Replace(" ", string.Empty);
            Debug.Assert(xx.Equals(yy));

            return Content(owlResponseString, MediaTypeNames.Application.Json);
        }

        //todo: colour scheme per relationship prefix, so more obvious what's ncs and what's esco
        // ^^ or colour scheme per node with children and the node's children?
        //todo: add namespace esco|ncs under text in node
        //todo: set relationship width so shows all

        //public class Neo4jResponse
        //{
        //    public string[] Keys { get; set; }
        //    //public int Length { get; set; }
        //    public Neo4jObject[] _Fields { get; set; }
        //    //_fieldLookup
        //}

        //public class Neo4jObject
        //{
        //    public ulong Id { get; set; }

        //    // public static Neo4jObject Create()
        //    // {
        //    //     if (labels != null)
        //    //         return
        //    // }
        //}

        //public class Node : Neo4jObject
        //{
        //    public string[] Labels { get; set; }
        //    public Dictionary<string, object> Properties { get; set; }
        //}

        //public class Relationship : Neo4jObject
        //{
        //    public ulong StartNodeId { get; set; }
        //    public ulong EndNodeId { get; set; }
        //    public string Type { get; set; }
        //    public Dictionary<string, object> Properties { get; set; }
        //}

        //public class Identity
        //{
        //    public uint Low { get; set; }
        //    public uint High { get; set; }
        //}

        //public class Neo4jObjectJsonConverter : JsonConverter<Neo4jObject>
        //{
        //    public override bool CanConvert(Type type)
        //    {
        //        return typeof(Neo4jObject).IsAssignableFrom(type);
        //    }

        //    public override Neo4jObject Read(
        //        ref Utf8JsonReader reader,
        //        Type typeToConvert,
        //        JsonSerializerOptions options)
        //    {
        //        if (reader.TokenType != JsonTokenType.StartObject)
        //        {
        //            throw new JsonException();
        //        }

        //        Identity identity = null;
        //        string[] labels = null;
        //        string type = null;

        //        while (reader.Read())
        //        {
        //            switch (reader.TokenType)
        //            {
        //                case JsonTokenType.PropertyName:
        //                    string propertyName = reader.GetString();
        //                    switch (propertyName)
        //                    {
        //                        case "identity":
        //                            identity = JsonSerializer.Deserialize<Identity>(ref reader);
        //                            break;
        //                        case "labels":
        //                            labels = JsonSerializer.Deserialize<string[]>(ref reader);
        //                            break;
        //                        case "type":
        //                            reader.Read();
        //                            type = reader.GetString();
        //                            break;
        //                        default:
        //                            throw new NotSupportedException();
        //                    }
        //                    break;
        //            }
        //        }

        //        //todo: identity combine into uint
        //        if (labels != null)
        //        {
        //            return new Node
        //            {
        //                Id = identity?.Low ?? throw new JsonException(),
        //                Labels = labels
        //            };
        //        }

        //        if (type != null)
        //        {
        //            return new Relationship
        //            {
        //                Id = identity?.Low ?? throw new JsonException(),
        //                Type = type
        //            };
        //        }

        //        throw new JsonException();

        //        // if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
        //        // {
        //        //     throw new JsonException();
        //        // }
        //    }

        //    public override void Write(
        //        Utf8JsonWriter writer,
        //        Neo4jObject value,
        //        JsonSerializerOptions options)
        //    {
        //        writer.WriteStartObject();

        //        switch (value)
        //        {
        //            case Node node:
        //                JsonSerializer.Serialize(writer, node);
        //                break;
        //            case Relationship relationship:
        //                JsonSerializer.Serialize(writer, relationship);
        //                break;
        //            default:
        //                throw new NotSupportedException();
        //        }

        //        writer.WriteEndObject();
        //    }
        //}

        //todo: private wwwroot files private need to be in this module

        public async Task<string> OriginalData(string uri)
        {
            const string prefLabel = "skos__prefLabel";
            var query = new GetNodesCypherQuery(nameof(uri), uri, prefLabel, prefLabel);
            _ = await _neoGraphDatabase.RunReadQuery(query);

            var minNodeId = query.Nodes.Keys.Min() - 1;
            var minRelationshipId = query.Relationships.Count > 0 ? query.Relationships.Min(r => r.Id) - 1 : 0;

            //from settings global...
            // ""zoom"": ""2.09"",
            // ""translation"": [
            // -1087.15,
            // -750.73
            //     ],


            var response = new StringBuilder($@"{{
           ""namespace"": [
            {{
             ""name"": ""NCS namespace"",
             ""iri"": ""https://nationalcareers.service.gov.uk""
            }}
            ],
           ""header"": {{
             ""languages"": [
               ""en""
             ],
             ""title"": {{
               ""en"": ""National Careers Service - Service Taxonomy""
             }},
             ""iri"": ""https://nationalcareers.service.gov.uk/test/"",
             ""version"": ""0.0.1 (alpha)"",
             ""author"": [
               ""National Careers Service""
             ],
             ""description"": {{
               ""en"": ""National Careers Service - Service Taxonomy""
             }}
           }},
           ""settings"": {{
             ""global"": {{
               ""paused"": false
             }},
             ""gravity"": {{
               ""classDistance"": 200,
               ""datatypeDistance"": 120
             }},
             ""filter"": {{
               ""checkBox"": [
                 {{
                   ""id"": ""datatypeFilterCheckbox"",
                   ""checked"": false
                 }},
                 {{
                   ""id"": ""objectPropertyFilterCheckbox"",
                   ""checked"": false
                 }},
                 {{
                   ""id"": ""subclassFilterCheckbox"",
                   ""checked"": false
                 }},
                 {{
                   ""id"": ""disjointFilterCheckbox"",
                   ""checked"": true
                 }},
                 {{
                   ""id"": ""setoperatorFilterCheckbox"",
                   ""checked"": false
                 }}
               ],
               ""degreeSliderValue"": ""0""
             }},
             ""options"": {{
               ""dynamicLabelWidth"": 120
             }},
             ""modes"": {{
               ""colorSwitchState"": true,
               ""checkBox"": [
                 {{
                   ""id"": ""editorModeModuleCheckbox"",
                   ""checked"": true
                 }},
                 {{
                   ""id"": ""pickandpinModuleCheckbox"",
                   ""checked"": false
                 }},
                 {{
                   ""id"": ""showZoomSliderConfigCheckbox"",
                   ""checked"": false
                 }},
                 {{
                   ""id"": ""labelWidthModuleCheckbox"",
                   ""checked"": false
                 }},
                 {{
                   ""id"": ""nodescalingModuleCheckbox"",
                   ""checked"": true
                 }},
                 {{
                   ""id"": ""compactnotationModuleCheckbox"",
                   ""checked"": false
                 }},
                 {{
                   ""id"": ""colorexternalsModuleCheckbox"",
                   ""checked"": true
                 }}
               ]
             }}
           }},
           ""class"": [");

            // {
            //       ""id"": ""Class1"",
            //       ""type"": ""owl:Class""
            //     }

            response.AppendJoin(',', query.Nodes.Select(n =>
                $"{{ \"id\": \"Class{n.Key - minNodeId}\", \"type\": \"owl:{(n.Key == query.SelectedNodeId ? "equivalent" : "")}Class\" }}"));

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

            // different colour
            // "attributes": [
            // "external"
            //     ],

            //description
            // "comment": {
            //     "en": "An automatic tag is a tag that is automatically associated with a resource (e.g. by a tagging system), i.e. it is not entered by a human being."
            // },


            Dictionary<string, string> typeColours = new Dictionary<string, string>();
            var ncsColourScheme = new ColourScheme(new string[] {
                             "#A6EBC9",
                             "#EDFFAB",
                             "#BCE7FD",
                             "#C7DFC5",
                             "#C1DBE3",
                             "#F3C178",
                             "#E2DBBE"
                         });

            var escoColourScheme = new ColourScheme(new string[] {
                        "#FFE5D4",
                        "#EFC7C2",
                        "#BA9593",
                        "#F7EDF0",
                    });


            response.AppendJoin(',', query.Nodes.Select(n =>
            {
                //                string type = n.Value.Labels.First(l => l != "Resource");
                string type = n.Value.Labels.First(l => l.StartsWith("ncs__")
    || l == "esco__Occupation"
    || l == "esco__Skill");
                string label;
                if (type.StartsWith("ncs__"))
                {
                    label = (string)n.Value.Properties[prefLabel];
                }
                else
                {
                    //   label = (string)((List<object>)n.Value.Properties[prefLabel]).First();
                    label = (string)n.Value.Properties[prefLabel];
                }

                // string comment = n.Value.Properties.ContainsKey("ncs__Description")
                //     ? $@",
                // ""comment"": {{
                //     ""en"": ""{(string)n.Value.Properties["ncs__Description"]}""
                // }}"
                //     : string.Empty;

                string comment = n.Value.Properties.ContainsKey("ncs__Description")
        ? (string)n.Value.Properties["ncs__Description"]
        : string.Empty;

                var classAttribute = new ClassAttributeToBeDeleted(
                    $"Class{n.Key - minNodeId}",
                    $"https://nationalcareers.service.gov.uk/test/{type}",
                    "https://nationalcareers.service.gov.uk/test/",
                    label,
                    comment);

                if (typeColours.ContainsKey(type))
                {
                    classAttribute.StaxBackgroundColour = typeColours[type];
                }
                else
                {
                    classAttribute.StaxBackgroundColour = typeColours[type] =
                        type.StartsWith("esco__") ? escoColourScheme.NextColour() : ncsColourScheme.NextColour();
                }

                classAttribute.StaxProperties = n.Value.Properties.Where(p => p.Key != prefLabel)
                    .Select(p => $"{p.Key}:{p.Value}").ToList();

                // if (type == "ncs__JobProfile")
                // {
                // //     classAttribute.StaxBackgroundColour = staxBackgroundColour;
                // classAttribute.Attributes.Add("primary");
                // //     // classAttribute.Attributes.Add("external");
                // //     // classAttribute.StaxAttributes.Add("primary");
                // }

                return JsonSerializer.Serialize(classAttribute, JsonOptions);

                //                 return $@"{{
                //        ""id"": ""Class{n.Key - minNodeId}"",
                //        ""iri"": ""https://nationalcareers.service.gov.uk/test/{type}"",
                //        ""baseIri"": ""http://visualdataweb.org/newOntology/"",
                //        ""label"": ""{label}""
                //        {comment}
                // }}";
            }));

            response.Append("],\"property\": [");

            // {
            //     ""id"": ""objectProperty1"",
            //     ""type"": ""owl:ObjectProperty""
            // }

            response.AppendJoin(',', query.Relationships.Select(r => $@"{{
                    ""id"": ""objectProperty{r.Id - minRelationshipId}"",
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

            response.AppendJoin(',', query.Relationships.Select(r => $@"{{
               ""id"": ""objectProperty{r.Id - minRelationshipId}"",
               ""iri"": ""https://nationalcareers.service.gov.uk/test/{r.Type}"",
               ""baseIri"": ""https://nationalcareers.service.gov.uk/test"",
               ""label"": ""{r.Type}"",
               ""attributes"": [
               ],
               ""domain"": ""Class{r.StartNodeId - minNodeId}"",
               ""range"": ""Class{r.EndNodeId - minNodeId}""
        }}"));

            response.Append("]}");

            return response.ToString();
        }
    }

    public class ClassAttributeToBeDeleted
    {
        public ClassAttributeToBeDeleted(string id, string iri, string baseIri, string label, string comment)
        {
            Id = id;
            Iri = iri;
            BaseIri = baseIri;
            Label = label;
            Comment = comment;
        }

        public string Id { get; set; }
        public string Iri { get; set; }
        public string BaseIri { get; set; }
        public string Label { get; set; }
        public string Comment { get; set; }
        public List<string> Attributes { get; set; } = new List<string>();

        public string StaxBackgroundColour { get; set; }
        public List<string> StaxProperties { get; set; } = new List<string>();

        //        public List<string> StaxAttributes { get; set; } = new List<string>();


        //todo: this form required?
        //     ""comment"": {{
        //     ""en"": ""{(string)n.Value.Properties["ncs__Description"]}""
        // }}"
    }
}
