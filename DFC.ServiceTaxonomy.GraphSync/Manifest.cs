using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Graph Sync",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.2",
    Description = "Enables syncing content to a Neo4j graph.",
    Category = "Graph",
    Dependencies = new[] { "OrchardCore.Contents", "OrchardCore.ContentTypes" }
)]
