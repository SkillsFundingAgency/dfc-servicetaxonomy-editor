using System;
using System.Collections.Generic;
using System.Text;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces
{
    public interface ISkillsFrameworkService
    {
        IEnumerable<OnetSkill> GetRelatedSkillMapping(string onetOccupationalCode);
    }
}
