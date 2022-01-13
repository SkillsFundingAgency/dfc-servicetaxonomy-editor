using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YesSql;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.DataAccess.Repositories
{
    public class GenericIndexRepository<T> : IGenericIndexRepository<T> where T : MapIndex
    {
        private readonly ISession _session;

        public GenericIndexRepository(ISession session)
        {
            _session = session;
        }

        public Task<int> GetCount()
        {
            return _session.QueryIndex<T>().CountAsync();
        }

        public Task<int> GetCount(Expression<Func<T, bool>> whereExpression)
        {
            return _session.QueryIndex<T>().Where(whereExpression).CountAsync();
        }

        public Task<T> FirstOrDefault(Expression<Func<T, bool>> whereExpression)
        {
            return _session.QueryIndex<T>().Where(whereExpression).FirstOrDefaultAsync();
        }

        public IQueryIndex<T> GetAll()
        {
            return _session.QueryIndex<T>();
        }

        public IQueryIndex<T> GetAll(Expression<Func<T, bool>> whereExpression)
        {
            return _session.QueryIndex<T>().Where(whereExpression);
        }

        public IQueryIndex<T> GetAll(Expression<Func<T, bool>> whereExpression, Expression<Func<T, object>> orderBy, bool descending)
        {
            return @descending ? _session.QueryIndex<T>().Where(whereExpression).OrderByDescending(orderBy) : _session.QueryIndex<T>().Where(whereExpression).OrderBy(orderBy);
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}
