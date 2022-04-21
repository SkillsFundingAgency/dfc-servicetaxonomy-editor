using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Graph Sync",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.3",
    Description = "Enables syncing content to a Cosmos db (referred to as graph).",
    Category = "Data Sync",
    Dependencies = new[] { "OrchardCore.Contents", "OrchardCore.ContentTypes", "DFC.ServiceTaxonomy.Content", "DFC.ServiceTaxonomy.JobProfiles.Module" }
)]
