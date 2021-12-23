using System.Collections.Generic;


namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
    public class WhatItTakesData
    {
        public IEnumerable<SkillItem>? RelatedSkills { get; set; }

        public string? RelatedDigitalSkills { get; set; }

        public string? OtherRequirements { get; set; }

        public IEnumerable<RestrictionItem>? RelatedRestrictions { get; set; }
    }
}
