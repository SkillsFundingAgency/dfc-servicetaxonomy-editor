using System;
using System.Linq;
using System.Linq.Expressions;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces
{
    public interface IQueryRepository<T>
        where T : class
    {
        // Get an entity by int id
        T GetById(string id);

        // Get an entity using delegate
        T Get(Expression<Func<T, bool>> where);

        // Gets all entities of type T
        IQueryable<T> GetAll();

        // Gets entities using delegate
        IQueryable<T> GetMany(Expression<Func<T, bool>> where);
    }
}
