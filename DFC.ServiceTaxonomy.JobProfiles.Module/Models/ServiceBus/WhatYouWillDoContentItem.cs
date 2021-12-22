namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
    public class WhatYouWillDoContentItem : RelatedContentItem
    {
        public string Description { get; set; } = string.Empty;

        public bool IsNegative { get; set; }

        public string Url { get; set; } = string.Empty;
    }
}
