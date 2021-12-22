using System;


namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
#pragma warning disable S101 // Types should be named in PascalCase
    public class WYDRelatedContentType
#pragma warning restore S101 // Types should be named in PascalCase
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Url { get; set; }

        public bool IsNegative { get; set; }
    }
}
