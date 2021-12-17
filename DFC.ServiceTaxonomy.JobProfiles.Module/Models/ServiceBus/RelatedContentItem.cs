using System;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
    internal class RelatedContentItem
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public Guid JobProfileId { get; set; }

        public string? JobProfileTitle { get; set; }
    }
}
