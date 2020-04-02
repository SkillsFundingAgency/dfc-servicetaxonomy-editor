namespace DFC.ServiceTaxonomy.Neo4j.Models.Interface
{
    public interface INeoDriver
    {
        INeoDriver Driver { get; set; }
        string Type { get; set; }
        string Uri { get; set; }
    }
}
