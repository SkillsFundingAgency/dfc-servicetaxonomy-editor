using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Banners",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.2",
    Description = "Provides support for handling banners in the STAX editor",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.ContentManagement.Display" }
)]
