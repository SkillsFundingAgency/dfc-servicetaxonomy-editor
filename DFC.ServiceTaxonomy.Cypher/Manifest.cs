using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Cypher Queries - new",
    Author = "National Careers Service",
    Website = "https://dfc-dev-stax-editor-as.azurewebsites.net",
    Version = "0.0.1",
    Description = "Enables Cypher queries to a Neo4j graph.",
    Category = "Graph",
    Dependencies = new[] { "OrchardCore.Contents" }
)]
