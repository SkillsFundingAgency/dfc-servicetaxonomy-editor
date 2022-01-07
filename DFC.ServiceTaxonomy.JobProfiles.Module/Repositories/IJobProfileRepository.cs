using System;
using System.Linq.Expressions;
using YesSql;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Repositories
{
    public interface IJobProfileRepository<T> where T : MapIndex
    {
        IQueryIndex<T> GetAll(Expression<Func<T, bool>> whereExpression);
    }
}
