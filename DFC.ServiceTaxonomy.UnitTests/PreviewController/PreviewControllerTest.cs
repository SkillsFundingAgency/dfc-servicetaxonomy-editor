using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.CustomEditor.Constants;
using DFC.ServiceTaxonomy.CustomEditor.Controllers;
using DfE.NCS.Framework.Core.Crypto.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.PreviewController
{
    public class PreviewControllerTest
    {
        private readonly StaxPreviewController _staxPreviewController;
        private readonly Mock<ILogger<StaxPreviewController>> _logger;
        private readonly Mock<ICryptographyManager> _cryptographyManager;
        private readonly IConfiguration _configuration;

        public PreviewControllerTest()
        {
            _logger = new Mock<ILogger<StaxPreviewController>>();
            _cryptographyManager = new Mock<ICryptographyManager>();

            var appSettings = new Dictionary<string, string?>
            {
                {ConfigKeys.StaxPreviewUrl, "https://test.com" },
                {ConfigKeys.CipherTextValiditySeconds, "60" },
                {ConfigKeys.CipherTextPrefix, "Test" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettings)
                .Build();
            _staxPreviewController = new StaxPreviewController(_configuration, _logger.Object, _cryptographyManager.Object);
        }

        [Fact]
        public void Preview_returnsExpected()
        {
            // Arrange
            _cryptographyManager.Setup(x => x.EncryptString(It.IsAny<string>())).Returns("test");

            // Act
            var result = _staxPreviewController.GotoUrl("test") as ContentResult;

            // Assert
            
            Assert.Contains("&redirect=test", result!.Content!);
        }

        [Fact]
        public void Preview_ThrowsNullError()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => _staxPreviewController.GotoUrl("test") as ContentResult);

            _logger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("exception")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
