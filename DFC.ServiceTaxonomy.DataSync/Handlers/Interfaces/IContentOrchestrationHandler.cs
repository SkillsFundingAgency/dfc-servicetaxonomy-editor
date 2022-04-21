using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.DataSync.Handlers.Interfaces
{
    public interface IContentOrchestrationHandler
    {
        Task DraftSaved(IOrchestrationContext context);
        Task Published(IOrchestrationContext context);
        Task Unpublished(IOrchestrationContext context);
        Task Cloned(IOrchestrationContext context);
        Task Deleted(IOrchestrationContext context);
        Task DraftDiscarded(IOrchestrationContext context);
        Task Restored(IOrchestrationContext context);
    }
}
