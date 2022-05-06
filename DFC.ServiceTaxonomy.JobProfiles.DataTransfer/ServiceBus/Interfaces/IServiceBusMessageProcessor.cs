using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces
{
    internal interface IServiceBusMessageProcessor
    {
        Task SendJobProfileMessage(JobProfileMessage jpData, string contentType, string actionType);
        Task SendOtherRelatedTypeMessages(IEnumerable<RelatedContentItem> relatedContentItems, string contentType, string actionType);
    }
}
