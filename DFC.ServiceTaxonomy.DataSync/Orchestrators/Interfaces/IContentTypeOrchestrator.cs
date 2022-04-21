using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.DataSync.Orchestrators.Interfaces
{
    public interface IContentTypeOrchestrator
    {
        Task DeleteItemsOfType(string contentTypeName);
        Task RemovePartFromItemsOfType(string contentTypeName, string contentPartName);
        Task RemoveFieldFromItemsWithPart(string contentPartName, string contentFieldName);
    }
}
