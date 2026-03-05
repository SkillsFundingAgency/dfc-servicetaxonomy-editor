using System;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Taxonomies.Settings
{
    public class TaxonomyContentsAdminListSettings 
    {
        public string[] TaxonomyContentItemIds { get; set; } = Array.Empty<string>();
    }
}
