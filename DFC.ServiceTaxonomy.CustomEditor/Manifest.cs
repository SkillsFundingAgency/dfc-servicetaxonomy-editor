using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Customising the STAX editor controls",
    Author = "National Careers Service",
    Website = "https://dfc-dev-stax-editor-as.azurewebsites.net",
    Version = "0.0.1",
    Description = "Enables custom overrides of standard orchard core controls.",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.Contents", "DFC.ServiceTaxonomy.ContentApproval", "DFC.ServiceTaxonomy.PageLocation" }
)]
