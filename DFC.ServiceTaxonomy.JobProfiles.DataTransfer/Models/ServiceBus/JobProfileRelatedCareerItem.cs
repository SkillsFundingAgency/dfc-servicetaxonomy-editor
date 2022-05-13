using System;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    internal class JobProfileRelatedCareerItem
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? ProfileLink { get; set; }
    }
}
