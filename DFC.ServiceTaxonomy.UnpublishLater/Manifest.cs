using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Unpublish Later",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.1",
    Description = "The Unpublish Later module adds the ability to schedule content items to be unpublished at a given future date and time.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
