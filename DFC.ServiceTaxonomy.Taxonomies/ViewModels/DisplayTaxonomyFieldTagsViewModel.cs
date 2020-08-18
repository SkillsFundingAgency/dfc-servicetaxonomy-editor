using DFC.ServiceTaxonomy.Taxonomies.Fields;

namespace DFC.ServiceTaxonomy.Taxonomies.ViewModels
{
    public class DisplayTaxonomyFieldTagsViewModel : DisplayTaxonomyFieldViewModel
    {
        public string[] TagNames => Field.GetTagNames();
    }
}
