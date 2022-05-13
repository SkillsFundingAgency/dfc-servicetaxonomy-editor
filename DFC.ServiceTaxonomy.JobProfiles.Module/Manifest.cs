using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Job profiles content handling",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.1",
    Description = "Provides content handling for job profiles skills.",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.Contents" }
)]
