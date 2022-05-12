using System.Threading.Tasks;

using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;

using OrchardCore.ContentManagement.Handlers;


namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Handlers
{
    internal class ServiceBusContentHandler : ContentHandlerBase
    {
        private readonly IDataEventProcessor _dataEventProcessor;

        public ServiceBusContentHandler(IDataEventProcessor dataEventProcessor)
        {
            _dataEventProcessor = dataEventProcessor;
        }

        public override async Task DraftSavedAsync(SaveDraftContentContext context)
            => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Draft);
        public override async Task PublishedAsync(PublishContentContext context)
            => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Published);
        public override async Task UnpublishedAsync(PublishContentContext context)
            => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Deleted);
        public override async Task RemovedAsync(RemoveContentContext context)
            => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Deleted);

    }
}
