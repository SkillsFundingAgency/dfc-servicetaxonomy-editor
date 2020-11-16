using DFC.ServiceTaxonomy.GraphSync.Notifications;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces
{
    public interface IOrchestrationContext
    {
        ContentItem ContentItem { get; }
        IGraphSyncNotifier Notifier { get; }

        bool Cancel { get; set; }
    }
}
