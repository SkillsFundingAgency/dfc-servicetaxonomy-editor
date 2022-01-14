using System;
using System.Collections.Generic;


namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
    public class SocSkillMatrixItem
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Contextualised { get; set; }

        public string? ONetAttributeType { get; set; }

        public decimal? Rank { get; set; }

        public decimal? ONetRank { get; set; }

        public IEnumerable<FrameworkSkillItem>? RelatedSkill { get; set; } 

        public IEnumerable<RelatedSocCodeItem>? RelatedSOC { get; set; } 
    }
}
