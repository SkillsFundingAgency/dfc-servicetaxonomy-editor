using System.Collections.Generic;
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

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }
    }
}
