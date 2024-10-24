using System;
using System.Collections.Generic;
using System.Text;

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
