using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Repositories
{
    public class SocMappingRepository : ISocMappingRepository
    {
        private readonly DfcDevOnetSkillsFrameworkContext _dbContext;

        public SocMappingRepository(DfcDevOnetSkillsFrameworkContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> UpdateMappingAsync(string socCode, string onetId)
        {
            var onetData = _dbContext.OccupationData.AsQueryable().FirstOrDefault(o => o.OnetsocCode == onetId);
            if (onetData == null)
            {
                // If we don't find a valid ONet data id then we can't map skills
                return false;
            }

            var socMapping = _dbContext.DfcSocMappings.AsQueryable().FirstOrDefault(s => s.SocCode == socCode);
            if(socMapping == null)
            {
                // Add a new soc code - onet id mapping
                await _dbContext.DfcSocMappings.AddAsync(new DfcSocMapping { SocCode = socCode, OnetCode = onetData.OnetsocCode, JobProfile = onetData.Title, QualityRating = 0, UpdateStatus = "UpdateCompleted" });
            }
            else if(socMapping.OnetCode != onetId)
            {
                // Update and existing soc code with a new onet id mapping
                socMapping.OnetCode = onetData.OnetsocCode;
                socMapping.JobProfile = onetData.Title;
                _dbContext.DfcSocMappings.Update(socMapping);
            }
            else
            {
                // Soc code - onet id mapping already existis
                return true;
            }

            var resultCount = await _dbContext.SaveChangesAsync();

            return resultCount == 1;
        }
    }
}
