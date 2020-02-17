using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Editor
{
    public class VisjsModel : PageModel
    {
        private readonly IGraphDatabase _neoGraphDatabase;

        public VisjsModel(IGraphDatabase neoGraphDatabase)
        {
            _neoGraphDatabase = neoGraphDatabase ?? throw new ArgumentNullException(nameof(neoGraphDatabase));
        }

        public async Task OnGet()
        {
            var nodes = new Dictionary<long, INode>();
            var relationships = new HashSet<IRelationship>();
            var fetch = "http://nationalcareers.service.gov.uk/jobprofile/c5397aee-4b53-4f62-aaec-b311f8c17332";
            await _neoGraphDatabase.RunReadQuery(
                new Query(
                    $"match (n:ncs__JobProfile {{uri:\"{fetch}\"}})-[r:ncs__hasSocCode|:ncs__hasDayToDayTask|:ncs__relatedOccupation]-(d) optional match (d)-[r1:esco__relatedEssentialSkill|:esco__relatedOptionalSkill]-(d1) return n, d, r, r1, d1"),
                r =>
                {
                    var sourceNode = r["n"].As<INode>();
                    var destNode = r["d"].As<INode>();
                    relationships.Add(r["r"].As<IRelationship>());

                    nodes[sourceNode.Id] = sourceNode;
                    nodes[destNode.Id] = destNode;

                    var otherNode = r["d1"].As<INode>();

                    if (otherNode != null)
                        nodes[otherNode.Id] = otherNode;

                    var otherRelationship = r["r1"].As<IRelationship>();

                    if (otherRelationship != null)
                        relationships.Add(otherRelationship);

                    //todo:
                    return 0;
                });

            Nodes = nodes.Select(x => new Node
            {
                Id = x.Key,
                Label = x.Value.Properties.Single(p => p.Key == "skos__prefLabel").Value is List<object>
                    ? ((List<object>)x.Value.Properties.Single(p => p.Key == "skos__prefLabel").Value)[0].ToString() ?? string.Empty
                    : x.Value.Properties.Single(p => p.Key == "skos__prefLabel").Value.ToString() ?? string.Empty,
                Type = x.Value.Labels.Any(c => c.StartsWith("ncs"))
                    ? x.Value.Labels.FirstOrDefault(c => c.StartsWith("ncs"))
                    : x.Value.Labels.LastOrDefault(c => c.StartsWith("esco")),
                Properties = x.Value.Properties.Select(c => new KeyValuePair<string, string>(c.Key, c.Value.ToString() ?? string.Empty)).ToList()
            }).ToList();

            Edges = relationships.Select(e => new Edge
            {
                From = e.StartNodeId,
                To = e.EndNodeId,
                Title = e.Type
            }).ToList();

            var routesIn = new dynamic[]
            {
                new
                {
                    Id = -1,
                    Label = "University",
                    Subjects = new dynamic[]
                    {
                        new
                        {
                            Id = -5,
                            Label = "3D design"
                        },
                        new
                        {
                            Id = -6,
                            Label = "product design"
                        },
                        new
                        {
                            Id = -7,
                            Label = "engineering"
                        },
                        new
                        {
                            Id = -8,
                            Label = "materials science"
                        }
                    },
                    EntryRequirements = new dynamic[]
                    {
                        new
                        {
                            Id = -13,
                            Label = "1 or 2 A levels, or equivalent, for a foundation degree or higher national diploma"
                        },
                        new
                        {
                            Id = -14,
                            Label = "2 to 3 A levels, or equivalent, for a degree"
                        }
                    }
                },
                new
                {
                    Id = -2,
                    Label = "College",
                    Subjects = new dynamic[]
                    {
                        new
                        {
                            Id = -9,
                            Label = "Level 2 Certificate in Computer-Aided Design and Manufacturing"
                        },
                        new
                        {
                            Id = -10,
                            Label = "Level 3 Certificate in 3D Design"
                        },
                        new
                        {
                            Id = -11,
                            Label = "Level 3 Diploma in Engineering Technology"
                        },
                        new
                        {
                            Id = -12,
                            Label = "T level in Digital Production, Design and Development"
                        }
                    },
                    EntryRequirements = new dynamic[]
                    {
                        new
                        {
                            Id = -15,
                            Label = "4 or 5 GCSEs at grades 9 to 4 (A* to C), or equivalent, including English, maths and computing"
                        },
                        new
                        {
                            Id = -16,
                            Label = "4 or 5 GCSEs at grades 9 to 4 (A* to C), or equivalent, including English and maths for a T level"
                        }
                    }
                },
                new
                {
                    Id = -3,
                    Label = "Apprenticeship",
                    Subjects = new string[0],
                    EntryRequirements = new dynamic[]
                    {
                        new
                        {
                            Id = -17,
                            Label = "5 GCSEs at grades 9 to 4 (A* to C), or equivalent, including English and maths, for an advanced apprenticeship"
                        }
                    }
                },
                new
                {
                    Id = -4,
                    Label = "Work",
                    Subjects = new string[0],
                    EntryRequirements = new dynamic[]
                    {
                        new
                        {
                            Id = -18,
                            Label = "You may be able to start as an assistant in a 3D print workshop and take training on the job to become a technician. Skills and qualifications in model making, printing, technology or design will be useful."
                        }
                    }
                }
            };

            foreach (var route in routesIn)
            {
                Nodes.Add(new Node
                {
                    Id = route.Id,
                    Label = route.Label,
                    Type = "ncs__routeIn"
                });

                Edges.Add(new Edge
                {
                    From = route.Id,
                    To = Nodes.Single(x => x.Type.Contains("JobProfile")).Id,
                    Title = "ncs__hasRouteIn"
                });

                if (route.Subjects.Length > 0)
                {
                    foreach (var subject in route.Subjects)
                    {
                        if (subject == null)
                            continue;

                        Nodes.Add(new Node
                        {
                            Id = subject.Id,
                            Label = subject.Label ?? string.Empty,
                            Type = "ncs__routeInSubject"
                        });

                        Edges.Add(new Edge
                        {
                            From = route.Id,
                            To = subject.Id,
                            Title = "ncs__hasRouteInSubject"
                        });
                    }
                }

                if (route.EntryRequirements.Length > 0)
                {
                    foreach (var entry in route.EntryRequirements)
                    {
                        if (entry == null)
                            continue;

                        Nodes.Add(new Node
                        {
                            Id = entry.Id,
                            Label = entry.Label ?? string.Empty,
                            Type = "ncs__routeInEntryRequirement"
                        });

                        Edges.Add(new Edge
                        {
                            From = route.Id,
                            To = entry.Id,
                            Title = "ncs__hasRouteInEntryRequirement"
                        });
                    }
                }
            }

            Nodes.ForEach(x =>
            {
                x.Group = x.Type;
                x.Mass = Edges.Count(e => e.From == x.Id || e.To == x.Id) * 3;
                x.Title = x.Label;

                if (x.Label.Length > 15)
                    x.Label = x.Label.Substring(0, 15) + "...";

                if (x.Properties.Any())
                {
                    x.Properties.ForEach(prop =>
                    {
                        x.ModalContents += $@"
                            <p>
                                <b>{prop.Key}</b>
                            </p>
                            <p>
                                {prop.Value}
                            </p>
                        ";
                    });
                }
            });
        }

        public Dictionary<string, string> Legend = new Dictionary<string, string>
        {
            { "ncs__JobProfile", "#eb7df4" },
            { "ncs__DayToDayTask", "#fb7e81" },
            { "ncs__SOCCode", "#7be141" },
            { "esco__occupation", "#ffff00" },
            { "esco_Skill", "#97c2fc" },
            { "ncs__routeIn", "#ad85e4" },
            { "ncs__routeInSubject", "#ffa807" },
            { "ncs__routeInEntryRequirement", "#6e6efd" }
        };

        public List<Node> Nodes { get; set; } = new List<Node>();

        public List<Edge> Edges { get; set; } = new List<Edge>();

        public class Node
        {
            public long Id { get; set; }
            public string Label { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public List<KeyValuePair<string, string>> Properties { get; set; } = new List<KeyValuePair<string, string>>();
            public int Mass { get; set; }
            public string Group { get; set; } = string.Empty;
            public string ModalContents { get; set; } = string.Empty;
        }

        public class Edge
        {
            public long From { get; set; }
            public long To { get; set; }
            public string Title { get; set; } = string.Empty;
            public int? Length { get; set; }
            public string Color { get; set; } = string.Empty;
        }
    }
}
