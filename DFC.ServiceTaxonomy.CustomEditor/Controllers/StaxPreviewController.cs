using System;
using DFC.ServiceTaxonomy.CustomEditor.Constants;
using DfE.NCS.Framework.Core.Crypto.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.CustomEditor.Controllers
{
    public class StaxPreviewController : Controller
    {
        private readonly IConfiguration _configuration;
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
            _logger.LogTrace($"Running preview url redirect for path: {path}");
            var previewLoginUrl = _configuration[ConfigKeys.StaxPreviewUrl];
            long validFromTicks = DateTime.UtcNow.Ticks;

            if (!long.TryParse(_configuration[ConfigKeys.CipherTextValiditySeconds], out var cipherValiditySeconds))
            {
                _logger.LogWarning($"Invalid value for: {ConfigKeys.CipherTextValiditySeconds}, setting it to default 60 seconds");
                cipherValiditySeconds = 60;
            }

            long validToTicks = DateTime.UtcNow.AddSeconds(cipherValiditySeconds).Ticks;
            string cipherText = $"{validFromTicks}:{_configuration[ConfigKeys.CipherTextPrefix]}:{validToTicks}";
            string encryptedText = _cryptographyManager.EncryptString(cipherText);
            string escapedString = Uri.EscapeDataString(encryptedText);
            var previewUrl = $"{previewLoginUrl}?data={escapedString}&redirect={path}";

            return Content(previewUrl);
        }
    }
}
