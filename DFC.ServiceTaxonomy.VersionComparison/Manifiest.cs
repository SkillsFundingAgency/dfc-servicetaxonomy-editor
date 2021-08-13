using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Version Comparison",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.1",
    Description = "Provides a version comparison admin page in the stax editor",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.ContentManagement", "OrchardCore.AuditTrail" }
)]
