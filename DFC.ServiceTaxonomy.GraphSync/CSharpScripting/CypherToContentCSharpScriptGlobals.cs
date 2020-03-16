using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.CSharpScripting
{
#pragma warning disable S1104
    public class CypherToContentCSharpScriptGlobals : ICypherToContentCSharpScriptGlobals
    {
        public IContentHelper Content { get; set; }

        public CypherToContentCSharpScriptGlobals(IContentHelper contentHelper)
        {
            Content = contentHelper;
        }
    }
#pragma warning restore S1104
}
