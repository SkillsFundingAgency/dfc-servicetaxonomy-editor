using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Job profiles module",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.1",
    Description = "Sets up and provides custom functionality for Job profiles in STAX.",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.Contents" }
)]
