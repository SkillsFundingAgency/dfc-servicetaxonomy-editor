using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Indexes;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.GraphQL
{
    public class JobProfileSimplificationPartIndexAliasProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
            {
                    new IndexAlias
                    {
                        Alias =nameof(JobProfileSimplificationPart),
                        Index = nameof(JobProfileIndex),
                        IndexType = typeof(JobProfileIndex)
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
