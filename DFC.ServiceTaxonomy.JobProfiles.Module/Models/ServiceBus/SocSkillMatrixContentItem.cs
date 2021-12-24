using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
    public class SocSkillMatrixContentItem : RelatedContentItem
    {
        public string Contextualised { get; set; } = string.Empty;

        public string ONetAttributeType { get; set; } = string.Empty;

        public decimal? Rank { get; set; }

        public decimal? ONetRank { get; set; }

        public FrameworkSkillItem? RelatedSkill { get; set; }

        public IEnumerable<RelatedSocCodeItem> RelatedSOC { get; set; } = new List<RelatedSocCodeItem>();

    }
}
