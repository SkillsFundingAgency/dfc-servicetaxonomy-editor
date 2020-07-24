using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Page Location",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.1",
    Description = "The Page Location module adds the PageLocationPart to specify and enforce unique URL's for content pages.",
    Dependencies = new[] { "OrchardCore.Contents", "OrchardCore.ContentManagement", "OrchardCore.DisplayManagement" },
    Category = "Content Management"
)]
