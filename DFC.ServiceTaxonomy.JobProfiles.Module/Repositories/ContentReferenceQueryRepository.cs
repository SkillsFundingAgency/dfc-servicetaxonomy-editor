using System;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Repositories
{
    public class ContentReferenceQueryRepository : IQueryRepository<FrameworkContent>
    {
        private readonly DfcDevOnetSkillsFrameworkContext onetDbContext;

        public ContentReferenceQueryRepository(DfcDevOnetSkillsFrameworkContext onetDbContext)
        {
            this.onetDbContext = onetDbContext;
        }

        public FrameworkContent Get(System.Linq.Expressions.Expression<Func<FrameworkContent, bool>> where)
        {
            throw new NotImplementedException();
        }

        public IQueryable<FrameworkContent> GetAll()
        {
            var result = from c in onetDbContext.ContentModelReferences.AsQueryable()
                         select new FrameworkContent()
                         {
                             ONetElementId = c.ElementId,
                             Title = c.ElementName
                         };

            return result;
        }

        public FrameworkContent GetById(string id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<FrameworkContent> GetMany(System.Linq.Expressions.Expression<Func<FrameworkContent, bool>> where)
        {
            throw new NotImplementedException();
        }
    }
}
