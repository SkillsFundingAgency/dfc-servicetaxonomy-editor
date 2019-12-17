using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Graph Sync",
    Author = "National Careers Service",
    Website = "https://dfc-dev-stax-editor-as.azurewebsites.net",
    Version = "0.0.1",
    Description = "Enables syncing content to a Neo4j graph.",
    Category = "Graph",
    Dependencies = new[] { "OrchardCore.Contents" }
)]
