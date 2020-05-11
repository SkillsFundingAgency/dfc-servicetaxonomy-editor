using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "DFC.ServiceTaxonomy.CustomFields",
    Author = "National Careers Service",
    Website = "https://dfc-dev-stax-editor-as.azurewebsites.net",
    Version = "0.0.1",
    Description = "Enables custom Tab / Accordion fields for turning content parts into tabs or accordions.",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.Contents", "OrchardCore.ContentTypes", "OrchardCore.ResourceManagement" }
)]
