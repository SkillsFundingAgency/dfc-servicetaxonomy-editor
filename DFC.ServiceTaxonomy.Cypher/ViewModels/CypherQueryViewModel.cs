namespace DFC.ServiceTaxonomy.Cypher.ViewModels
{
    public class CypherQueryViewModel
    {
        public string? Query { get; set; }

        public string? Parameters { get; set; }

        public string? ResultModelType { get; set; }

        public bool ReturnDocuments { get; set; }
    }
}
