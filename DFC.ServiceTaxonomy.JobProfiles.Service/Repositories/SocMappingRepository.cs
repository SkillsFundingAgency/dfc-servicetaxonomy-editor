using System;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Service.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Repositories
{
    public class SocMappingRepository : ISocMappingRepository
    {
        private readonly DfcDevOnetSkillsFrameworkContext _dbContext;

        public SocMappingRepository(DfcDevOnetSkillsFrameworkContext dbContext)
        {
            _dbContext = dbContext;
        }

        public SocCodeMapping GetById(string id)
        {
            var socMapping = _dbContext.DfcSocMappings.AsQueryable().FirstOrDefault(s => s.SocCode == id);
            if (socMapping == null)
            {
                throw new ArgumentException($"{id} not found in SOC code mappings table");
            }

            return new SocCodeMapping
            {
                SOCCode = id,
                ONetOccupationalCode = socMapping.OnetCode,
                Description = socMapping.JobProfile
            };
        }
    }
}
