using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Taxonomies.Validation
{
    public interface ITaxonomyTermValidator
    {
        Task<bool> Validate(ContentItem term, ContentItem taxonomy);
        string ErrorMessage { get; }
    }
}
