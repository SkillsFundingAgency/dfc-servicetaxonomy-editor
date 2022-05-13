using OrchardCore.Modules.Manifest;

[assembly: Module(
    Id = "DFC.ServiceTaxonomy.Content",
    Name = "STAX Content",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.1",
    Description = "Registers Stax Content Item helper services with the DI container.",
    Category = "Graph",
    Dependencies = new[] { "OrchardCore.Contents" }
)]
