using System;
using System.Collections.Generic;


namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public class SocCodeItem
    {
        public Guid Id { get; set; }

        public string? SOCCode { get; set; }

        public string? Description { get; set; }

        public string? ONetOccupationalCode { get; set; }

        public string? SOC2020 { get; set; }

        public string? SOC2020extension { get; set; }

        public IEnumerable<Classification>? ApprenticeshipFramework { get; set; }

        public IEnumerable<Classification>? ApprenticeshipStandards { get; set; }
    }
}
