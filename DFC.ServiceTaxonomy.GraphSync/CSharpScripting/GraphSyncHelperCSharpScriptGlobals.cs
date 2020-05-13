﻿using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.CSharpScripting
{
    public class GraphSyncHelperCSharpScriptGlobals : IGraphSyncHelperCSharpScriptGlobals
    {
        public IContentHelper Content { get; }
        public IServiceTaxonomyHelper ServiceTaxonomy { get; }    // put namespace in here, or global?
        public string? Value { get; set; }
        public string? ContentType { get; set; }

        public GraphSyncHelperCSharpScriptGlobals(
            IContentHelper contentHelper,
            IServiceTaxonomyHelper serviceTaxonomy)
        {
            Content = contentHelper;
            ServiceTaxonomy = serviceTaxonomy;
        }
    }
}
