using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Media.Configuration;
using Microsoft.Azure.Management.Cdn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using OrchardCore.Media.Core.Events;
using OrchardCore.Media.Events;

namespace DFC.ServiceTaxonomy.Media.Events
{
    public class MediaBlobStoreEventHandler : MediaEventHandlerBase
    {
        private const string Resource = "https://management.core.windows.net/";
        private const string AssetsRequestPath = "/media";
        private readonly ILogger<MediaBlobStoreEventHandler> _logger;
        private readonly AzureAdSettings _azureAdSettings;

        public MediaBlobStoreEventHandler(ILogger<MediaBlobStoreEventHandler> logger, IOptions<AzureAdSettings> azureAdSettings)
        {
            _azureAdSettings = azureAdSettings.Value;
            _logger = logger;
        }

        public override async Task MediaDeletedFileAsync(MediaDeletedContext context)
        {
            await PurgeCdnAsync($"{AssetsRequestPath}/{context.Path}");
        }

        private async Task<bool> PurgeCdnAsync(string contentPath)
        {
            bool hasErrors = false;

            try
            {
                AuthenticationResult authenticationResult = await GetAccessToken();

                if (authenticationResult == null)
                    return hasErrors;

                CdnManagementClient cdnManagementClient = new CdnManagementClient(new TokenCredentials(authenticationResult.AccessToken))
                {
                    SubscriptionId = _azureAdSettings.SubscriptionId
                };

                cdnManagementClient.Endpoints.PurgeContent(_azureAdSettings.ResourceGroupName,
                                                           _azureAdSettings.CdnProfileName,
                                                           _azureAdSettings.CdnEndpointName,
                                                           new List<string>() { contentPath });
                hasErrors = false;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error purging media content:{contentPath}");
                hasErrors = true;
            }

            return hasErrors;
        }

        private async Task<AuthenticationResult> GetAccessToken()
        {
            AuthenticationResult authenticationResult = null;

            try
            {
                AuthenticationContext authContext = new AuthenticationContext(_azureAdSettings.Authority);

                ClientCredential credential = new ClientCredential(_azureAdSettings.ClientId, _azureAdSettings.ClientSecret);
                authenticationResult = await authContext.AcquireTokenAsync(Resource, credential);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error getting access token");
            }

            return authenticationResult;
        }
    }
}
