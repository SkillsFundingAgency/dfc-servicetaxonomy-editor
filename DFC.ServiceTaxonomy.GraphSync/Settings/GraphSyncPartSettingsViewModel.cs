using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsViewModel
    {
        public string MySetting { get; set; }

        [BindNever]
        public GraphSyncPartSettings GraphSyncPartSettings { get; set; }
    }
}
