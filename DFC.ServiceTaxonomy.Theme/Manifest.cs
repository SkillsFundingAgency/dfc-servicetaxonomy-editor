using OrchardCore.DisplayManagement.Manifest;

//todo: tags & basetheme? https://orchardcore.readthedocs.io/en/dev/docs/guides/create-admin-theme/

[assembly: Theme(
    Name = "Stax Theme",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.1",
    Description = "Service taxonomy editor theme.",
    Dependencies = new[] { "OrchardCore.ResourceManagement" }
)]
