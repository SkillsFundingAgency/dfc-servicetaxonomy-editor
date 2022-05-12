using System;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Repositories
{
    public class SuppressionsQueryRepository : IQueryRepository<FrameworkSkillSuppression>
    {
        private readonly DfcDevOnetSkillsFrameworkContext onetDbContext;

        public SuppressionsQueryRepository(DfcDevOnetSkillsFrameworkContext onetDbContext)
        {
            this.onetDbContext = onetDbContext;
        }

        #region Implementation of IQueryRepository<FrameworkSkillSuppression>
        public FrameworkSkillSuppression Get(System.Linq.Expressions.Expression<Func<FrameworkSkillSuppression, bool>> where)
        {
            throw new NotImplementedException();
        }

        public IQueryable<FrameworkSkillSuppression> GetAll()
        {
            var result = (from s in onetDbContext.DfcGlobalAttributeSuppressions.AsQueryable()
                          select new FrameworkSkillSuppression()
                          {
                              ONetElementId = s.OnetElementId
                          });

            return result;
        }

        public FrameworkSkillSuppression GetById(string id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<FrameworkSkillSuppression> GetMany(System.Linq.Expressions.Expression<Func<FrameworkSkillSuppression, bool>> where)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
