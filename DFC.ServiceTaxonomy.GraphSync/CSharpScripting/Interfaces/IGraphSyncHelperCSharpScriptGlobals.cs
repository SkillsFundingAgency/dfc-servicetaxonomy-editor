
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces
{
    public interface IGraphSyncHelperCSharpScriptGlobals
    {
        public IContentHelper Content { get; }
        string? Value { get; set; }
        string? ContentType { get; set; }
    }
}
