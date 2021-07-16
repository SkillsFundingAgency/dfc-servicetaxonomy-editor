using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Media.Services;
using OrchardCore.Media.Core.Events;
using OrchardCore.Media.Events;

namespace DFC.ServiceTaxonomy.Media.Events
{
    public class MediaBlobStoreEventHandler : MediaEventHandlerBase
    {
        private const string AssetsRequestPath = "/media";
        private readonly ICdnService _cdnService;

        public MediaBlobStoreEventHandler(ICdnService cdnService)
        {
            _cdnService = cdnService;
        }

        public override Task MediaDeletedFileAsync(MediaDeletedContext context)
        {
            _cdnService.PurgeContentAsync(new List<string>() { $"{AssetsRequestPath}/{context.Path}" });

            return Task.CompletedTask;
        }
    }
}
