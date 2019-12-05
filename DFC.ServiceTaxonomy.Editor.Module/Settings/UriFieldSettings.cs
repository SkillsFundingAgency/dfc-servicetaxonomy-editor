using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Editor.Module.Settings
{
    public class UriFieldSettings
    {
        public string? NamespacePrefix { get; set; }
        //todo: want defaults on NamespacePrefixConfiguration, not here
        public List<string> NamespacePrefixOptions { get; set; } = new List<string> {string.Empty};
    }
}
