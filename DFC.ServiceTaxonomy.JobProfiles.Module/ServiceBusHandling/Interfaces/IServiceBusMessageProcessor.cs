using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces
{
    internal interface IServiceBusMessageProcessor
    {
        Task SendJobProfileMessage(JobProfileMessage jpData, string contentType, string actionType);
        Task SendOtherRelatedTypeMessages(IEnumerable<RelatedContentItem> relatedContentItems, string contentType, string actionType);
    }
}
