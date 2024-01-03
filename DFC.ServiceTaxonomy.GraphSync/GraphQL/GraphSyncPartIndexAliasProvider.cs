using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Indexes;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.GraphSync.GraphQL
{
    public class GraphSyncPartIndexAliasProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
        {
            new IndexAlias
            {
                Alias = "graphSyncPart",
                Index = "GraphSyncPartIndex",
                IndexType = typeof(GraphSyncPartIndex)
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }
    }
}
