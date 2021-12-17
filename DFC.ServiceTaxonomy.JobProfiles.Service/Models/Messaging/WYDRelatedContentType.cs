using System;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Models.Messaging
{
    public class WYDRelatedContentType
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public bool IsNegative { get; set; }
    }
}
