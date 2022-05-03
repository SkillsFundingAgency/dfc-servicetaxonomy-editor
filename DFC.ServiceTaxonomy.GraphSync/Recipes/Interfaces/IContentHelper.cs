using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Interfaces
{
    public interface IContentHelper
    {
        Task<string> GetContentItemIdByDisplayText(string contentType, string displayText);
    }
}
