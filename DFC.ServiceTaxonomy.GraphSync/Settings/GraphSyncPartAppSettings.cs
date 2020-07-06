using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsConfiguration
    {
        public GraphSyncPartSettingsConfiguration()
        {
            Settings = new List<GraphSyncPartSettings>();
        }

        public List<GraphSyncPartSettings> Settings { get; set; }
    }
}
