
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces
{
    public interface ISyncNameProviderCSharpScriptGlobals
    {
        public IContentHelper Content { get; }
        string? Value { get; set; }
        string? ContentType { get; set; }
    }
}
