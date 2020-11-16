using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Contexts
{
    public class OrchestrationContext : IOrchestrationContext
    {
        public ContentItem ContentItem { get; }
        // do we want to grant direct access?
        public IGraphSyncNotifier Notifier { get; }

        public bool Cancel { get; set; }

        public OrchestrationContext(
            ContentItem contentItem,
            IGraphSyncNotifier notifier)
        {
            Notifier = notifier;
            ContentItem = contentItem;
        }
    }
}
