using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Approval",
    Author = "National Careers Service",
    Website = "https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor",
    Version = "0.0.2",
    Description = "Allows content items to require approval before publishing and adds item status dashboard widgets",
    Category = "Content Management",
    Dependencies = new[]
    {
        "OrchardCore.AdminDashboard",
        "OrchardCore.ContentManagement",
        "OrchardCore.ContentPreview",
        "OrchardCore.Contents",
        "OrchardCore.ContentTypes.Abstractions",
        "OrchardCore.ContentDisplayManagement",
        "DFC.ServiceTaxonomy.Content"
    }
)]
