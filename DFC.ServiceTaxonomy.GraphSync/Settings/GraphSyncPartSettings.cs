using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettings
    {
        public string? NamespacePrefix { get; set; }
        //todo: [BindNever]
        public List<string>? NamespacePrefixOptions { get; set; }
    }
}
