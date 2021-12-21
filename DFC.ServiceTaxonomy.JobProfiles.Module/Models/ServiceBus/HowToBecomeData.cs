using System.Collections.Generic;


namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
    public class HowToBecomeData
    {
        public IEnumerable<RouteEntryItem>? RouteEntries { get; set; }

        public MoreInformation? FurtherInformation { get; set; }

        public FurtherRoutes? FurtherRoutes { get; set; }

        public string? IntroText { get; set; }

        public IEnumerable<RegistrationItem>? Registrations { get; set; }
    }
}
