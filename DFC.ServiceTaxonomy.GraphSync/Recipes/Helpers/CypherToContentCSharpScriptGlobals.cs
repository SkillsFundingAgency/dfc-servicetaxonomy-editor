using DFC.ServiceTaxonomy.GraphSync.Recipes.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Helpers
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
