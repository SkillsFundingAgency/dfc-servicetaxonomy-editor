using System;
using System.Linq.Expressions;
using YesSql;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Repositories
{
    public class JobProfileRepository<T> : IJobProfileRepository<T> where T : MapIndex
    {
        private readonly ISession _session;

        public JobProfileRepository(ISession session)
        {
            _session = session;
        }

        public IQueryIndex<T> GetAll(Expression<Func<T, bool>> whereExpression)
        {
            return _session.QueryIndex<T>().Where(whereExpression);
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}
