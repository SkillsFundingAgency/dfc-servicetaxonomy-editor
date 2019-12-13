using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.GraphLookup.Settings
{
    public class GraphLookupPartSettingsViewModel
    {
        public string MySetting { get; set; }

        [BindNever]
        public GraphLookupPartSettings GraphLookupPartSettings { get; set; }
    }
}
