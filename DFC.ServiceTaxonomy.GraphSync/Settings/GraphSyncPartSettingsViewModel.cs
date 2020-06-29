
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsViewModel
    {
        public GraphSyncPartSettingsViewModel()
        {
            Settings = new List<SelectListItem>();
            AllSettings = new List<GraphSyncPartSettings>();
        }

        public List<SelectListItem> Settings { get; set; }
        public List<GraphSyncPartSettings> AllSettings { get; set; }
        public string? SelectedSetting { get; set; }
        public string? BagPartContentItemRelationshipType { get; set; }
        public bool PreexistingNode { get; set; }
        public string? NodeNameTransform { get; set; }
        public string? PropertyNameTransform { get; set; }
        public string? CreateRelationshipType { get; set; }    //or RelationshipTypeTransform for consistency?
        public string? IdPropertyName { get; set; }
        public string? GenerateIdPropertyValue { get; set; }
        public bool DisplayId { get; set; }
        public bool ReadOnly { get; internal set; }
    }
}
