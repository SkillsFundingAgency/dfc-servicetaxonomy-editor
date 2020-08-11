using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "STAX Taxonomies",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "DFC.ServiceTaxonomy.Taxonomies",
    Name = "STAX Taxonomies",
    Description = "The taxonomies module provides a way to categorize content items.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "DFC.ServiceTaxonomy.Taxonomies.ContentsAdminList",
    Name = "STAX Taxonomies Contents List Filters",
    Description = "Provides taxonomy filters in the contents list.",
    Category = "Content Management"
)]
