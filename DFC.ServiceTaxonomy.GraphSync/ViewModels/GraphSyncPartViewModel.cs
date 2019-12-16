using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;

namespace DFC.ServiceTaxonomy.GraphSync.ViewModels
{
    public class GraphSyncPartViewModel
    {
        public string MySetting { get; set; }

        public bool Show { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public GraphSyncPart GraphSyncPart { get; set; }

        [BindNever]
        public GraphSyncPartSettings Settings { get; set; }
    }
}
