using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphVisualiser.Models;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Configuration;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Services
{
    public abstract class OwlDataGeneratorService
    {
        private readonly Dictionary<string, string> typeColours = new Dictionary<string, string>();

        private readonly OwlDataGeneratorConfigModel OwlDataGeneratorConfigModel;

        private readonly ColourScheme ncsColourScheme = new ColourScheme(new string[] {
                     "#A6EBC9",
                     "#EDFFAB",
                     "#BCE7FD",
                     "#C7DFC5",
                     "#C1DBE3",
                     "#F3C178",
                     "#E2DBBE"
                 });

        private readonly ColourScheme escoColourScheme = new ColourScheme(new string[] {
                "#FFE5D4",
                "#EFC7C2",
                "#BA9593",
                "#F7EDF0",
            });

        protected IEnumerable<NodeDataModel> nodeDataModels = new List<NodeDataModel>();

        protected IEnumerable<RelationshipDataModel> relationshipDataModels = new List<RelationshipDataModel>();

        protected OwlDataGeneratorService(Microsoft.Extensions.Options.IOptionsMonitor<OwlDataGeneratorConfigModel> owlDataGeneratorConfigModel)
        {
            OwlDataGeneratorConfigModel = owlDataGeneratorConfigModel?.CurrentValue ?? throw new ArgumentNullException(nameof(owlDataGeneratorConfigModel));
        }

        protected List<Namespace> CreateNamespaces()
        {
            var result = new List<Namespace>
            {
                new Namespace
                {
                    Name = OwlDataGeneratorConfigModel.NamespaceName,
                    Iri = OwlDataGeneratorConfigModel.NamespaceIri,
                },
            };

            return result;
        }

        protected Header CreateHeader()
        {
            var result = new Header
            {
                Languages = new List<string> { OwlDataGeneratorConfigModel.DefaultLanguage! },
                Title = CreateDescription(),
                Iri = OwlDataGeneratorConfigModel.HeaderIri!,
                Version = OwlDataGeneratorConfigModel.HeaderVersion!,
                Author = new List<string> { OwlDataGeneratorConfigModel.HeaderAuthor!, },
                Description = CreateDescription(),
            };

            return result;
        }

        private Description CreateDescription()
        {
            var result = new Description
            {
                En = OwlDataGeneratorConfigModel.DescriptionLabel,
            };

            return result;
        }

        protected Settings CreateSettings()
        {
            var result = new Settings
            {
                Global = new Global { Paused = false, },
                Gravity = new Gravity { ClassDistance = 200, DatatypeDistance = 120, },
                Filter = CreateFilter(),
                Options = new Options { DynamicLabelWidth = 120, },
                Modes = CreateModes(),
            };

            return result;
        }

        private Filter CreateFilter()
        {
            var result = new Filter
            {
                DegreeSliderValue = "0",
                CheckBox = new List<CheckBox>
                {
                    new CheckBox { Id = "datatypeFilterCheckbox", Checked = false, },
                    new CheckBox { Id = "objectPropertyFilterCheckbox", Checked = false, },
                    new CheckBox { Id = "subclassFilterCheckbox", Checked = false, },
                    new CheckBox { Id = "disjointFilterCheckbox", Checked = true, },
                    new CheckBox { Id = "setoperatorFilterCheckbox", Checked = false, },
                },
            };

            return result;
        }

        private Modes CreateModes()
        {
            var result = new Modes
            {
                ColorSwitchState = true,
                CheckBox = new List<CheckBox>
                {
                    new CheckBox{ Id = "editorModeModuleCheckbox", Checked = true,},
                    new CheckBox{ Id = "pickandpinModuleCheckbox", Checked = false,},
                    new CheckBox{ Id = "showZoomSliderConfigCheckbox", Checked = false,},
                    new CheckBox{ Id = "labelWidthModuleCheckbox", Checked = false,},
                    new CheckBox{ Id = "nodescalingModuleCheckbox", Checked = true,},
                    new CheckBox{ Id = "compactnotationModuleCheckbox", Checked = false,},
                    new CheckBox{ Id = "colorexternalsModuleCheckbox", Checked = true,},
                }
            };

            return result;
        }

        protected Class CreateClass(NodeDataModel nodeDataModel, string selectedNode)
        {
            var result = new Class
            {
                Id = nodeDataModel.Id,
                Type = $"owl:{(nodeDataModel.Id!.Equals("Class" + selectedNode) ? "equivalent" : string.Empty)}Class",
            };

            return result;
        }

        protected ClassAttribute CreateClassAttribute(NodeDataModel nodeDataModel)
        {
            var classAttribute = new ClassAttribute
            {
                Id = nodeDataModel.Id,
                Iri = $"https://nationalcareers.service.gov.uk/test/{nodeDataModel.Type}",
                BaseIri = "https://nationalcareers.service.gov.uk/test/",
                Label = nodeDataModel.Label,
                Comment = nodeDataModel.Comment,
                StaxBackgroundColour = GetNextColour(nodeDataModel.Type!),
                StaxProperties = nodeDataModel.StaxProperties,
            };

            return classAttribute;
        }

        protected Property CreateProperty(RelationshipDataModel relationshipDataModel)
        {
            var result = new Property
            {
                Id = $"objectProperty{relationshipDataModel.Id}",
                Type = "owl:ObjectProperty",
            };

            return result;
        }

        protected PropertyAttribute CreatePropertyAttribute(RelationshipDataModel relationshipDataModel)
        {
            var result = new PropertyAttribute
            {
                Id = $"objectProperty{relationshipDataModel.Id}",
                Iri = $"https://nationalcareers.service.gov.uk/test/{relationshipDataModel.Label}",
                BaseIri = "https://nationalcareers.service.gov.uk/test",
                Label = relationshipDataModel.Label,
                Domain = relationshipDataModel.Domain,
                Range = relationshipDataModel.Range
            };

            return result;
        }

        private string GetNextColour(string type)
        {
            string result;

            if (typeColours.ContainsKey(type))
            {
                result = typeColours[type];
            }
            else
            {
                result = typeColours[type] =
                        type.StartsWith("esco__") ? escoColourScheme.NextColour() : ncsColourScheme.NextColour();
            }

            return result;
        }
    }
}
