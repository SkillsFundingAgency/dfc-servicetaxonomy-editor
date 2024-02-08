using DFC.ServiceTaxonomy.PageLocation.Models;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.PageLocation.GraphQL
{
    public class PagePartIndexAliasProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
        {
            new IndexAlias
            {
                Alias = nameof(PagePart),
                Index = nameof(PageItemsPartIndex),
                IndexType = typeof(PageItemsPartIndex)
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }

    }
}
