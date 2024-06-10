using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.CompUi.BackgroundTask
{
    public class BackgroundItemQueueMonitor : IBackgroundItemQueueMonitor
    {
        private static readonly object LockObject = new object();

        private static bool isRunning = false;

        private readonly IBackgroundQueue<Processing> queue;
        private readonly ILogger<BackgroundItemQueueMonitor> logger;
        private readonly IJobProfileCacheRefresh jobProfileCacheRefresh;

        public BackgroundItemQueueMonitor(IBackgroundQueue<Processing> queue, ILogger<BackgroundItemQueueMonitor> logger, IJobProfileCacheRefresh jobProfileCacheRefresh)
        {
            this.queue = queue;
            this.logger = logger;
            this.jobProfileCacheRefresh = jobProfileCacheRefresh;
        }

        public void TryStart(CancellationToken cancellationToken)
        {
            lock (LockObject)
            {
                if (!isRunning)
                {
                    _ = Task.Run(() => DoBackgroundProcessing(cancellationToken)).ConfigureAwait(false);
                    isRunning = true;
                }
            }
        }

        private async Task DoBackgroundProcessing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var contentItem = await queue.DequeueItem(cancellationToken);

                try
                {
                    if (contentItem != null)
                    {
                        await jobProfileCacheRefresh.RefreshAllJobProfileContent(contentItem);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    break;
                }
            }

            lock (LockObject)
            {
                if (isRunning)
                {
                    isRunning = false;
                }
            }
        }

    }
}
