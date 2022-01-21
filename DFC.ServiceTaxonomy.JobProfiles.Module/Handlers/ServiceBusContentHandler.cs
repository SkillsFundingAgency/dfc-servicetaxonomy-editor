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

        public override Task DraftSavedAsync(SaveDraftContentContext context)
            => _dataEventProcessor.ProcessContentContext(context, ActionTypes.Draft);
        public override Task PublishedAsync(PublishContentContext context)
            => _dataEventProcessor.ProcessContentContext(context, ActionTypes.Published);
        public override Task UnpublishedAsync(PublishContentContext context)
            => _dataEventProcessor.ProcessContentContext(context, ActionTypes.Deleted);
        public override Task RemovedAsync(RemoveContentContext context)
            => _dataEventProcessor.ProcessContentContext(context, ActionTypes.Deleted);

    }
}
