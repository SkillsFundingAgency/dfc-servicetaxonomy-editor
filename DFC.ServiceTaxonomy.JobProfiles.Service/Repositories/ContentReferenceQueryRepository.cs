using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.JobProfiles.Service.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Repositories
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
            var result = (from c in onetDbContext.ContentModelReferences
                          select new FrameworkContent()
                          {
                              ONetElementId = c.ElementId,
                              Title = c.ElementName
                          });

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
