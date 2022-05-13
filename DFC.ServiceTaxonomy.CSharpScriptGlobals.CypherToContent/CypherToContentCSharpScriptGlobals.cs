using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;

namespace DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent
{
#pragma warning disable S1104
    public class CypherToContentCSharpScriptGlobals : ICypherToContentCSharpScriptGlobals
    {
        public IContentHelper Content { get; }

        public CypherToContentCSharpScriptGlobals(IContentHelper contentHelper)
        {
            Content = contentHelper;
        }
    }
#pragma warning restore S1104
}
