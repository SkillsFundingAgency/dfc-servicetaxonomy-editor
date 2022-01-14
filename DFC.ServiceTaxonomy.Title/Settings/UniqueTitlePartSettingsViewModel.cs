using System.Collections.Generic;

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
        public bool ReadOnlyOnPublish { get; set; }
    }
}
