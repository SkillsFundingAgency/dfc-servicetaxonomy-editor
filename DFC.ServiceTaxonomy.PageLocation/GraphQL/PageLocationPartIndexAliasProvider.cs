using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Indexes;
using DFC.ServiceTaxonomy.PageLocation.Models;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.PageLocation.GraphQL
{
    public class PageLocationPartIndexAliasProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
        {
            new IndexAlias
            {
                Alias = nameof(PageLocationPart),
                Index = nameof(PageLocationPartIndex),
                IndexType = typeof(PageLocationPartIndex)
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }

        public async ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
        {
            return _aliases;
        }
    }
}
