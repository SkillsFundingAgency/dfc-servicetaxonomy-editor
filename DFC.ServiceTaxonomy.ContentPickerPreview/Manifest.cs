using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "ContentPicker Preview Editor",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.2",
    Description = "Preview editor for ContentPicker fields.",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.ContentTypes" }
)]
