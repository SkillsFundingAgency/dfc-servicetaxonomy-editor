using System;
using System.Linq;
using System.Linq.Expressions;
using DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Repositories
{
    public class CombinationsQueryRepository : IQueryRepository<FrameworkSkillCombination>
    {
        private readonly DfcDevOnetSkillsFrameworkContext onetDbContext;

        public CombinationsQueryRepository(DfcDevOnetSkillsFrameworkContext onetDbContext)
        {
            this.onetDbContext = onetDbContext;
        }

        #region Implementation of IQueryRepository<FrameWorkSkillCombination>
        public FrameworkSkillCombination Get(Expression<Func<FrameworkSkillCombination, bool>> where)
        {
            throw new NotImplementedException();
        }

        public IQueryable<FrameworkSkillCombination> GetAll()
        {
            var result = (from c in onetDbContext.DfcGdscombinations.AsQueryable()
                          orderby c.ApplicationOrder
                          select new FrameworkSkillCombination()
                          {
                              OnetElementOneId = c.OnetElementOneId,
                              OnetElementTwoId = c.OnetElementTwoId,
                              Title = c.ElementName,
                              Description = c.Description,
                              CombinedElementId = c.CombinedElementId
                          });

            return result;

        }

        public FrameworkSkillCombination GetById(string id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<FrameworkSkillCombination> GetMany(Expression<Func<FrameworkSkillCombination, bool>> where)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
