﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Media.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace DFC.ServiceTaxonomy.Media.Services
{
    [ExcludeFromCodeCoverage]
    public class TokenService : ITokenService
    {
        private const string Resource = "https://management.core.windows.net/";
        private readonly ILogger<TokenService> _logger;
        private readonly AzureAdSettings _azureAdSettings;
        private readonly IKeyVaultService _keyVaultService;

        public TokenService(IKeyVaultService keyVaultService, IOptions<AzureAdSettings> azureAdSettings, ILogger<TokenService> logger)
        {
            _keyVaultService = keyVaultService;
            _azureAdSettings = azureAdSettings.Value;
            _logger = logger;
        }

        public async Task<string> GetAccessToken()
        {
            AuthenticationResult authenticationResult;
            try
            {
                AuthenticationContext authContext = new AuthenticationContext(_azureAdSettings.Authority);

                string clientId = await _keyVaultService.GetSecrectAsync(_azureAdSettings.ClientId);

                string clientSecret = await _keyVaultService.GetSecrectAsync(_azureAdSettings.ClientSecret);

                ClientCredential credential = new ClientCredential(clientId, clientSecret);

                authenticationResult = await authContext.AcquireTokenAsync(Resource, credential);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error getting access token");
                return null;
            }

            return authenticationResult.AccessToken;
        }
    }
}
