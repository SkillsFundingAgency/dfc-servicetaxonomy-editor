using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "ContentPicker Preview",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.1",
    Description = "ContentPicker with preview part.",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.ContentTypes" }
)]
