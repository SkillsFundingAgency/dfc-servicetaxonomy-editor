using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Approval",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.2",
    Description = "Enables a content approval workflow and related permissions",
    Category = "Content Management",
    Dependencies = new[]
    {
        "OrchardCore.ContentManagement",
        "OrchardCore.Contents"
    }
)]
