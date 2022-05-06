using System;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public class MoreInformationLinkItem
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public Uri? Url { get; set; }

        public string? Text { get; set; }
    }
}
