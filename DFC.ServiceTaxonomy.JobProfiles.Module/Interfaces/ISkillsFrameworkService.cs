using System.Collections.Generic;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces
{
    public interface ISkillsFrameworkService
    {
        IEnumerable<OnetSkill> GetRelatedSkillMapping(string onetOccupationalCode);
    }
}
