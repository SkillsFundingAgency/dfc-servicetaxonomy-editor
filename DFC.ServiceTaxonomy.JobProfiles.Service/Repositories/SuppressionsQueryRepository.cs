using System;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Service.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Repositories
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
            var result = (from s in onetDbContext.DfcGlobalAttributeSuppressions
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
