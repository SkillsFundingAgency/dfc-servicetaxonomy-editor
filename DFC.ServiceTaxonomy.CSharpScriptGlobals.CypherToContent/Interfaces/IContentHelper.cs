using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces
{
    public interface IContentHelper
    {
        Task<string> GetContentItemIdByDisplayText(string contentType, string displayText);
    }
}
