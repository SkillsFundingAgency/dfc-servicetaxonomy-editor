
namespace DFC.ServiceTaxonomy.Editor.Module.Settings
{
    public class GraphLookupPartSettings
    {
        public string? NodeLabel { get; set; }
        //public List<string>? DisplayFieldName { get; set; }
        public string? DisplayFieldName { get; set; }
        public string? ValueFieldName { get; set; }
        public string? RelationshipType { get; set; }
        public bool NodesAreReadonly { get; set; }
    }
}
