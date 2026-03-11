using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YesSql;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.DataAccess.Repositories
{
    public interface IGenericIndexRepository<T> : IDisposable where T : MapIndex
    {
        Task<int> GetCount();

        Task<int> GetCount(Expression<Func<T, bool>> whereExpression);

        Task<T> FirstOrDefault(Expression<Func<T, bool>> whereExpression);

        IQueryIndex<T> GetAll();

        IQueryIndex<T> GetAll(Expression<Func<T, bool>> whereExpression);

        IQueryIndex<T> GetAll(Expression<Func<T, bool>> whereExpression, Expression<Func<T, Object>> orderBy, bool descending);

    }
}
