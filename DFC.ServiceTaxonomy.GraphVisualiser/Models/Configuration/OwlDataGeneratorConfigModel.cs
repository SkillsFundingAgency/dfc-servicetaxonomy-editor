namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Configuration
{
    public class OwlDataGeneratorConfigModel
    {
        public string DefaultLanguage { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string NamespaceIri { get; set; } = string.Empty;
        public string HeaderIri { get; set; } = string.Empty;
        public string HeaderAuthor { get; set; } = string.Empty;
        public string HeaderVersion { get; set; } = string.Empty;
        public string DescriptionLabel { get; set; } = string.Empty;
    }
}
