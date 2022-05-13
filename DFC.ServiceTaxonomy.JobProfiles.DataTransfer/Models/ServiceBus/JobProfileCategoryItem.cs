using System;


namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    internal class JobProfileCategoryItem
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string? Title { get; set; }
    }
}
