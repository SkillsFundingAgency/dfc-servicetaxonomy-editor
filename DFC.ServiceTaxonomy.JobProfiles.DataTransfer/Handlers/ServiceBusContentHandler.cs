using System.Threading.Tasks;

using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Handlers;


namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Handlers
{
    internal class ServiceBusContentHandler : ContentHandlerBase
    {
        private readonly IDataEventProcessor _dataEventProcessor;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServiceBusContentHandler(
            IDataEventProcessor dataEventProcessor,
            IHttpContextAccessor httpContextAccessor)
        {
            _dataEventProcessor = dataEventProcessor;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task PublishedAsync(PublishContentContext context)
            => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Published);
        public override async Task UnpublishedAsync(PublishContentContext context)
            => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Deleted);
        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            if (!ValidateEventFailure(context, ContextTypes.SaveDraftContentContext))
            {
                await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Draft);
            }
        }

        public override async Task RemovedAsync(RemoveContentContext context)
        {
            if (!ValidateEventFailure(context, ContextTypes.RemoveContentContext))
            {
                await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Deleted);

            }
        }

        private bool ValidateEventFailure(ContentContextBase context, string contextType)
        {
            if(context.ContentItem.ContentType != ContentTypes.JobProfile)
            {
                return false;
            }
            return _httpContextAccessor.HttpContext.Items.TryGetValue(context.ContentItem.ContentItemId, out var result) &&
                result.ToString() == contextType;
        }
    }
}
