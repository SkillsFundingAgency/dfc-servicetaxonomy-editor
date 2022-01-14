using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.ServiceTaxonomy.Title.Settings
{
    public class UniqueTitlePartSettingsViewModel
    {
        public UniqueTitlePartSettingsViewModel()
        {
            AllSettings = new List<UniqueTitlePartSettings>();
        }
        public List<UniqueTitlePartSettings> AllSettings { get; set; }
        public string? Hint { get; set; }
        public string? Placeholder { get; set; }
    }
}
