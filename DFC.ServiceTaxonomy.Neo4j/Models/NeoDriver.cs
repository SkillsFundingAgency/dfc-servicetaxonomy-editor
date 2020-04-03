using DFC.ServiceTaxonomy.Neo4j.Models.Interface;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Models
{
    public class NeoDriver : INeoDriver
    {
        public NeoDriver(IDriver driver, string? uri)
        {
            this.Driver = driver;
            this.Uri = uri;
        }

        public IDriver Driver { get; set; }
        public string? Uri { get; set; }
    }
}
