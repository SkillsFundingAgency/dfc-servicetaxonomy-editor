using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Models.Interface
{
    public interface INeoDriver
    {
        IDriver Driver { get; set; }
        string Type { get; set; }
        string? Uri { get; set; }
    }
}
