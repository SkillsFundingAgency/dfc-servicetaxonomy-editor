using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using OrchardCore.ContentManagement.Handlers;


namespace DFC.ServiceTaxonomy.JobProfiles.Module.Handlers
{
    internal class ServiceBusContentHandler : ContentHandlerBase
    {
        private readonly IDataEventProcessor _dataEventProcessor;

        public ServiceBusContentHandler(IDataEventProcessor dataEventProcessor)
        {
            _dataEventProcessor = dataEventProcessor;
        }

        public override async Task DraftSavedAsync(SaveDraftContentContext context) => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Draft);

        public override async Task PublishingAsync(PublishContentContext context) => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Published);

        public override async Task UnpublishingAsync(PublishContentContext context) => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Deleted);

        // State          Action     Context:latest      published     no active version left
        // Pub            Delete            0            0                1
        // Pub+Draft      Discard Draft     0            0                0
        // Draft          Delete            0            0                1
        // Pub+Draft      Delete            0            0                1
        public override async Task RemovingAsync(RemoveContentContext context) => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Deleted);

        public override async Task PublishedAsync(PublishContentContext context) => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Published);
        public override async Task UnpublishedAsync(PublishContentContext context) => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Deleted);
        public override async Task RemovedAsync(RemoveContentContext context) => await _dataEventProcessor.ProcessContentContext(context, ActionTypes.Deleted);

    }
}
