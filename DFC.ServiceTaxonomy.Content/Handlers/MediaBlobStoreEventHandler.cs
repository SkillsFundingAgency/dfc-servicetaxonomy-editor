using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Media.Core.Events;
using OrchardCore.Media.Events;

namespace DFC.ServiceTaxonomy.Content.Handlers
{
    public class MediaBlobStoreEventHandler : MediaEventHandlerBase
    {
        private const string AssetsRequestPath = "/media";
        private readonly IConfiguration _configuration;
        private readonly ICdnService _cdnService;
        private readonly ILogger<MediaBlobStoreEventHandler> _logger;

        public MediaBlobStoreEventHandler(IConfiguration configuration, ICdnService cdnService, ILogger<MediaBlobStoreEventHandler> logger)
        {
            _configuration = configuration;
            _cdnService = cdnService;
            _logger = logger;
        }

        public override Task MediaDeletedFileAsync(MediaDeletedContext context)
        {
            if(_configuration.GetChildren().Any(item => item.Key == "AzureAdSettings"))
            {
                _cdnService.PurgeContentAsync(new List<string>() { $"{AssetsRequestPath}/{context.Path}" });
            }
            else
            {
                _logger.LogError("Failed to get config secttion: AzureAdSettings");
            }

            return Task.CompletedTask;
        }
    }
}
