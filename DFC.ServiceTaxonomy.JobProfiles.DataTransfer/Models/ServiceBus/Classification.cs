using System;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public class Classification
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Url { get; set; }

        public string? Description { get; set; }
    }
}
