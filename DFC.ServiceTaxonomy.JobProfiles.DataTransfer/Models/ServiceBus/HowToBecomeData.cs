using System.Collections.Generic;


namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public class HowToBecomeData
    {
        public IEnumerable<RouteEntryItem>? RouteEntries { get; set; }

        public MoreInformation? FurtherInformation { get; set; }

        public FurtherRoutes? FurtherRoutes { get; set; }

        public string? IntroText { get; set; }

        public IEnumerable<RegistrationItem>? Registrations { get; set; }

        /// <summary>
        /// Gets or sets the social proof real story when one has been selected for the job profile.
        /// </summary>
        /// <value>
        /// A <see cref="ServiceBus.RealStory"/> when present; otherwise, a value of <c>null</c>.
        /// </value>
        public RealStory? RealStory { get; set; }
    }
}
