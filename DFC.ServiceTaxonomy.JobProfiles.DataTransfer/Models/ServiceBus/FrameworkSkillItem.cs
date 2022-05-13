using System;


namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public class FrameworkSkillItem
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? ONetElementId { get; set; }

        public string? Description { get; set; }
    }
}
