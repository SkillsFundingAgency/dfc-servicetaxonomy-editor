using DFC.ServiceTaxonomy.CompUi.Interfaces;
using OrchardCore.BackgroundTasks;

namespace DFC.ServiceTaxonomy.CompUi.BackgroundTask
{
    [BackgroundTask(
        Title = "Refresh Cache on Publish Background Task",
        Schedule = "* * * * *",
        Description = "Refresh cache on publish background task")]
    public class RefreshCacheOnPublish : IBackgroundTask
    {
        private readonly IBackgroundItemQueueMonitor backgroundQueueMonitor;
        public RefreshCacheOnPublish(IBackgroundItemQueueMonitor backgroundQueueMonitor)
        {
            this.backgroundQueueMonitor = backgroundQueueMonitor;
        }

        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            backgroundQueueMonitor.TryStart(cancellationToken);
            return Task.CompletedTask;
        }
    }
}
