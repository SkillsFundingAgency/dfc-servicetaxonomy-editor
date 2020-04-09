using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface IValidateAndRepairGraph
    {
        Task<bool> ValidateGraph();
        Task<bool> ValidateContentItem(ContentItem contentItem, ContentTypeDefinition contentTypeDefinition);
    }
}
