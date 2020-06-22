using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.UnpublishLater.Indexes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using YesSql;

namespace DFC.ServiceTaxonomy.UnpublishLater.Services
{
    [BackgroundTask(Schedule = "* * * * *", Description = "Unpublishes content items when their scheduled unpublish date time arrives.")]
    public class ScheduledUnpublishingBackgroundTask : IBackgroundTask
    {
        private readonly ILogger<ScheduledUnpublishingBackgroundTask> _logger;
        private readonly IClock _clock;

        public ScheduledUnpublishingBackgroundTask(ILogger<ScheduledUnpublishingBackgroundTask> logger, IClock clock)
        {
            _logger = logger;
            _clock = clock;
        }

        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var itemsToUnpublish = await serviceProvider
                .GetRequiredService<ISession>()
                .Query<ContentItem, UnpublishLaterPartIndex>(index => index.ScheduledUnpublishUtc < _clock.UtcNow)
                .ListAsync();

            if (!itemsToUnpublish.Any())
            {
                return;
            }

            foreach (var item in itemsToUnpublish)
            {
                _logger.LogDebug("Unpublishing scheduled content item {ContentItemId}.", item.ContentItemId);
                await serviceProvider.GetRequiredService<IContentManager>().UnpublishAsync(item);
            }
        }
    }
}
