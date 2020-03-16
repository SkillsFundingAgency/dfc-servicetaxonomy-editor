using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.CSharpScripting
{
    public class GraphSyncHelperCSharpScriptGlobals : IGraphSyncHelperCSharpScriptGlobals
    {
        public IContentHelper Content { get; }
        public IServiceTaxonomyHelper ServiceTaxonomy { get; }
        public string? Value { get; set; }

        public GraphSyncHelperCSharpScriptGlobals(
            IContentHelper contentHelper,
            IServiceTaxonomyHelper serviceTaxonomy)
        {
            Content = contentHelper;
            ServiceTaxonomy = serviceTaxonomy;
        }
    }
}
