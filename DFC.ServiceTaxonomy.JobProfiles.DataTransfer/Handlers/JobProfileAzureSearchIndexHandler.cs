using System.Threading.Tasks;

using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBusHandling;

using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Handlers
{
    public class JobProfileAzureSearchIndexHandler : ContentHandlerBase
    {
        private readonly IAzureSearchDataProcessor _azureSearchDataProcessor;

        public JobProfileAzureSearchIndexHandler(IAzureSearchDataProcessor azureSearchDataProcessor)
        {
            _azureSearchDataProcessor = azureSearchDataProcessor;
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            if(context.ContentItem.ContentType == ContentTypes.JobProfile) await _azureSearchDataProcessor.ProcessContentContext(context, ActionTypes.Published);
        }

        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            if (context.ContentItem.ContentType == ContentTypes.JobProfile) await _azureSearchDataProcessor.ProcessContentContext(context, ActionTypes.Deleted);
        }

        public override async Task RemovedAsync(RemoveContentContext context)
        {
            if (context.ContentItem.ContentType == ContentTypes.JobProfile) await _azureSearchDataProcessor.ProcessContentContext(context, ActionTypes.Deleted);
        }

    }
}
