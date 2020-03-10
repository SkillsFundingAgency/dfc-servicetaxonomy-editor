using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Cypher.ViewModels
{
    public class CypherQueryResultViewModel
    {
        public List<object> Results { get; set; }
        public CypherQueryIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CypherQueryIndexOptions
    {
        public string QueryName { get; set; }
        public string Search { get; set; }
        public string ItemViewModel { get; set; }
    }
}
