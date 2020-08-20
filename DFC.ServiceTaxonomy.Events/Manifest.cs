using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Events",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "1.1.0",
    Description = "Publishes content item status events to Azure Event Grid",
    Category = "Events",
    Dependencies = new[] { "OrchardCore.ContentManagement" }
)]
