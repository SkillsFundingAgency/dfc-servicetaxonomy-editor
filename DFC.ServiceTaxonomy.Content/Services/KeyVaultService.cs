using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DFC.ServiceTaxonomy.Content.Configuration;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.Content.Services
{
    [ExcludeFromCodeCoverage]
    public class KeyVaultService : IKeyVaultService
    {
        private readonly AzureAdSettings _azureAdSettings;
        private readonly ILogger<KeyVaultService> _logger;

        public KeyVaultService(IOptions<AzureAdSettings> azureAdSettings, ILogger<KeyVaultService> logger)
        {
            _azureAdSettings = azureAdSettings.Value;
            _logger = logger;
        }

        public async Task<string?> GetSecrectAsync(string keyVaultKey)
        {
            var vaultUri = new Uri(_azureAdSettings.KeyVaultAddress!);

            try
            {
                var secretClient = new SecretClient(vaultUri, new DefaultAzureCredential());

                Azure.Response<KeyVaultSecret> response = await secretClient.GetSecretAsync(keyVaultKey);

                return response.Value.Value;
            }
            catch (AuthenticationFailedException exception)
            {
                _logger.LogError($"Authentication Failed. {exception.Message}");

                return null;
            }
        }
    }
}
