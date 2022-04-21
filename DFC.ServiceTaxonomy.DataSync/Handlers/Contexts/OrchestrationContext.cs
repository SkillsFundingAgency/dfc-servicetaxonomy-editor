using DFC.ServiceTaxonomy.DataSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Notifications;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.Handlers.Contexts
{
    public class OrchestrationContext : IOrchestrationContext
    {
        public ContentItem ContentItem { get; }
        // do we want to grant direct access?
        public IDataSyncNotifier Notifier { get; }

        public bool Cancel { get; set; }

        public OrchestrationContext(
            ContentItem contentItem,
            IDataSyncNotifier notifier)
        {
            Notifier = notifier;
            ContentItem = contentItem;
        }
    }
}
