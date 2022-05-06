using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Job profiles data transfer and indexing module",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.1",
    Description = "Sets up handlers to send job profile messages and update indexes.",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.Contents" }
)]
