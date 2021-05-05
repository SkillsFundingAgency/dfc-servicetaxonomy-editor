using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Media.Configuration;
using Microsoft.Azure.Management.Cdn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;

namespace DFC.ServiceTaxonomy.Media.Services
{
    [ExcludeFromCodeCoverage]
    public class CdnService : ICdnService
    {
        private readonly ILogger<CdnService> _logger;
        private readonly AzureAdSettings _azureAdSettings;
        private readonly ITokenService _tokenService;

        public CdnService(ITokenService tokenService, IOptions<AzureAdSettings> azureAdSettings, ILogger<CdnService> logger)
        {
            _tokenService = tokenService;
            _azureAdSettings = azureAdSettings.Value;
            _logger = logger;
        }

        public async Task<bool> PurgeContent(IList<string> contentPaths)
        {
            bool hasErrors = false;

            try
            {
                string accessToken = await _tokenService.GetAccessToken();

                if (accessToken == null)
                {
                    _logger.LogError($"Error purging media content:{string.Join(",", contentPaths)}");
                    return hasErrors;
                }

                CdnManagementClient cdnManagementClient = new CdnManagementClient(new TokenCredentials(accessToken))
                {
                    SubscriptionId = _azureAdSettings.SubscriptionId
                };

                cdnManagementClient.Endpoints.PurgeContent(_azureAdSettings.ResourceGroupName,
                                                           _azureAdSettings.CdnProfileName,
                                                           _azureAdSettings.CdnEndpointName,
                                                           contentPaths);
                hasErrors = false;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error purging media content:{string.Join(",", contentPaths)}");
                hasErrors = true;
            }

            return hasErrors;
        }
    }
}
