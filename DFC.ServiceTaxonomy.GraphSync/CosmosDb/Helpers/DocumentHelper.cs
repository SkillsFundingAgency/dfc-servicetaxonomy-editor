using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Helpers
{
    public static class DocumentHelper
    {
        public static readonly IReadOnlyList<string> CosmosPropsToIgnore = new List<string>
        {
            "_rid",
            "_self",
            "_etag",
            "_attachments",
            "_ts",
            "_links"
        };
    }
}
