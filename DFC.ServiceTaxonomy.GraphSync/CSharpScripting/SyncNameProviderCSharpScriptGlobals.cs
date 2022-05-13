using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.CSharpScripting
{
    public class SyncNameProviderCSharpScriptGlobals : ISyncNameProviderCSharpScriptGlobals
    {
        public IContentHelper Content { get; }
        public IServiceTaxonomyHelper ServiceTaxonomy { get; }    // put namespace in here, or global?
        public string? Value { get; set; }
        public string? ContentType { get; set; }

        public SyncNameProviderCSharpScriptGlobals(
            IContentHelper contentHelper,
            IServiceTaxonomyHelper serviceTaxonomy)
        {
            Content = contentHelper;
            ServiceTaxonomy = serviceTaxonomy;
        }
    }
}
