using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Banners.Indexes;
using DFC.ServiceTaxonomy.Banners.Models;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.Banners.GraphQL
{
    public class BannerPartIndexAliasProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
        {
                new IndexAlias
                {
                    Alias =nameof(BannerPart),
                    Index = nameof(BannerPartIndex),
                    IndexType = typeof(BannerPartIndex)
                }
        };

        public async ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
        {
            await Task.Yield();
            return _aliases;
        }
    }
}
