using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsViewModel
    {
        public string? NamespacePrefix { get; set; }

        [BindNever]
        public List<string>? NamespacePrefixOptions { get; set; }
    }
}
