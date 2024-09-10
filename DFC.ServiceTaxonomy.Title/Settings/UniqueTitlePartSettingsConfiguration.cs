using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Title.Settings
{
    public class UniqueTitlePartSettingsConfiguration
    {
        public UniqueTitlePartSettingsConfiguration()
        {
            Settings = new List<UniqueTitlePartSettings>();
        }

        public List<UniqueTitlePartSettings> Settings { get; set; }
    }
}
