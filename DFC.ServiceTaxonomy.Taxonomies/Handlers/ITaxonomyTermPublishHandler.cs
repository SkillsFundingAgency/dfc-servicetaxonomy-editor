using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Taxonomies.Handlers
{
    public interface ITaxonomyTermPublishHandler
    {
        Task Publish(ContentItem contentItem);
    }
}
