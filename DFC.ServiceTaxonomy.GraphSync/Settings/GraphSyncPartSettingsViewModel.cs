
namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsViewModel
    {
        public string? BagPartContentItemRelationshipType { get; set; }
        public bool PreexistingNode { get; set; }
        public string? NodeNameTransform { get; set; }
        public string? PropertyNameTransform { get; set; }
        public string? CreateRelationshipType { get; set; }    //or RelationshipTypeTransform for consistency?
        public string? IdPropertyName { get; set; }
        public string? GenerateIdPropertyValue { get; set; }
    }
}
