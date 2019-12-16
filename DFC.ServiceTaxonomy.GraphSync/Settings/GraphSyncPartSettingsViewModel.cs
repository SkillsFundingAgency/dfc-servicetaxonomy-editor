using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsViewModel
    {
        public string? NamespacePrefix { get; set; }
        //todo: [BindNever]
        public List<string>? NamespacePrefixOptions { get; set; }

        // [BindNever]
        // public GraphSyncPartSettings GraphSyncPartSettings { get; set; }
    }
}
