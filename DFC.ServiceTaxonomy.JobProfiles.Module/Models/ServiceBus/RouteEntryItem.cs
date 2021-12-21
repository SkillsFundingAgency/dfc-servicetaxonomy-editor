using System.Collections.Generic;


namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
    public class RouteEntryItem
    {
        public RouteEntryType RouteName { get; set; }

        public IEnumerable<EntryRequirementItem>? EntryRequirements { get; set; }

        public IEnumerable<MoreInformationLinkItem>? MoreInformationLinks { get; set; }

        public string? RouteSubjects { get; set; }

        public string? FurtherRouteInformation { get; set; }

        public string? RouteRequirement { get; set; }
    }
}
