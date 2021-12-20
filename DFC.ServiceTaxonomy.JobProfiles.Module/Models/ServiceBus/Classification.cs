﻿using System;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
    internal class Classification
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Url { get; set; }

        public string? Description { get; set; }
    }
}
