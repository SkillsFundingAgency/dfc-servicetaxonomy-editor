using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Models.Messaging
{
    public class WhatYouWillDoData
    {
        public string DailyTasks { get; set; }

        public IEnumerable<WYDRelatedContentType> Locations { get; set; }

        public IEnumerable<WYDRelatedContentType> Uniforms { get; set; }

        public IEnumerable<WYDRelatedContentType> Environments { get; set; }

        public string Introduction { get; set; }
    }
}
