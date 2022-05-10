using System;


namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public class InfoDataItem
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Info { get; set; }
    }
}
