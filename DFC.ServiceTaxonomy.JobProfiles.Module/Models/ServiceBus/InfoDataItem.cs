using System;


namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
    internal class InfoDataItem
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Info { get; set; }
    }
}
