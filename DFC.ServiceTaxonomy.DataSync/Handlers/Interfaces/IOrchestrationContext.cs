using DFC.ServiceTaxonomy.DataSync.Notifications;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.Handlers.Interfaces
{
    public interface IOrchestrationContext
    {
        ContentItem ContentItem { get; }
        IDataSyncNotifier Notifier { get; }

        bool Cancel { get; set; }
    }
}
