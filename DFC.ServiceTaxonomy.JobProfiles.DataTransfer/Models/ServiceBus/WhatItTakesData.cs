using System.Collections.Generic;


namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public class WhatItTakesData
    {
        public IEnumerable<SocSkillMatrixItem>? RelatedSocSkillMatrixSkills { get; set; }

        public string? RelatedDigitalSkills { get; set; }

        public string? OtherRequirements { get; set; }

        public IEnumerable<RestrictionItem>? RelatedRestrictions { get; set; }
    }
}
