using System;
using DfE.NCS.Framework.Core.Crypto.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.CustomEditor.Controllers
{
    public class StaxPreviewController : Controller
    {
        private readonly IConfiguration _configuration;
        private const string staxPreviewUrl = "NcsPreview:PreviewLoginUrl";
        private readonly ILogger<StaxPreviewController> _logger;
        private readonly ICryptographyManager _cryptographyManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaxPreviewController"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public StaxPreviewController(IConfiguration config, ILogger<StaxPreviewController> logger, ICryptographyManager cryptographyManager)
        {
            _configuration = config;
            _logger = logger;
            _cryptographyManager = cryptographyManager;
        }

        public ActionResult GotoUrl(string path)
        {
            var staxPreviewLoginUrl = _configuration[staxPreviewUrl];
            long validFromTicks = DateTime.UtcNow.Ticks;
            if (!long.TryParse(_configuration["NcsPreview:CipherTextValiditySeconds"], out var cipherValiditySeconds))
            {
                _logger.LogWarning("Invalid value for 'NcsPreview:CypherTextValiditySeconds' setting it to default 60 seconds");
                cipherValiditySeconds = 60;
            }
            long validToTicks = DateTime.UtcNow.AddSeconds(cipherValiditySeconds).Ticks;
            string cipherText = $"{validFromTicks}:{_configuration["NcsPreview:CipherTextPrefix"]}:{validToTicks}";
            string encryptedText = _cryptographyManager.EncryptString(cipherText);
            string escapedString = Uri.EscapeDataString(encryptedText);
            var url = $"{staxPreviewLoginUrl}?data={escapedString}&redirect={path}";
            _logger.LogWarning("Do the dew");
            return Content(url);
        }
    }
}
