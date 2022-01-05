using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Job Profile Index",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.2",
    Description = "Provides support for Job Profile fields in the STAX editor",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.ContentManagement.Display" }
)]
