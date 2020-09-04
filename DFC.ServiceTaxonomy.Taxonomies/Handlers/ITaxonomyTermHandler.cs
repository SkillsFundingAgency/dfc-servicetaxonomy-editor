using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Taxonomies.Handlers
{
    public interface ITaxonomyTermHandler
    {
        Task UpdatedAsync(ContentItem term, ContentItem taxonomy);
    }
}
