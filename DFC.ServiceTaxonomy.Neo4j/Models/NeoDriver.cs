using System;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Models
{
    public class NeoDriver
    {
        public NeoDriver(string type, IDriver driver, string? uri)
        {
            this.Driver = driver;
            this.Type = type;
            this.Uri = uri;
        }

        public IDriver Driver { get; set; }
        public string Type { get; set; }
        public string? Uri { get; set; }
    }
}
