using System.Collections.Generic;
using DFC.ServiceTaxonomy.Taxonomies.Fields;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Taxonomies.ViewModels
{
    public class EditTaxonomyFieldViewModel
    {
        public string UniqueValue { get; set; }
        public List<TermEntry> TermEntries { get; set; } = new List<TermEntry>();

        [BindNever]
        public ContentItem Taxonomy { get; set; }

        [BindNever]
        public TaxonomyField Field { get; set; }

        [BindNever]
        public ContentPart Part { get; set; }

        [BindNever]
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }

    public class TermEntry
    {
        [BindNever]
        public ContentItem Term { get; set; }

        public bool Selected { get; set; }
        public string ContentItemId { get; set; }

        [BindNever]
        public int Level { get; set; }

        public bool IsLeaf { get; set; }
    }
}
