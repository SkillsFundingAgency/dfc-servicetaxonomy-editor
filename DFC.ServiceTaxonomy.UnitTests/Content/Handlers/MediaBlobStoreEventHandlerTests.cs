using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Handlers;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Media.Events;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Content.Handlers
{
    public  class MediaBlobStoreEventHandlerTests
    {
        private const string ContentPath = "/media/testImage.png";
        private readonly ICdnService _fakeCdnService;
        private readonly ILogger<MediaBlobStoreEventHandler> _logger;

        public MediaBlobStoreEventHandlerTests()
        {
            _fakeCdnService = A.Fake<ICdnService>();
            _logger = A.Fake<ILogger<MediaBlobStoreEventHandler>>();
        }

        [Fact]
        public async Task MediaDeletedFileAsyncCallsCdnService()
        {
            //arrange
            var mediaDeletedContext = new MediaDeletedContext
            {
                Path = ContentPath,
                Result = true
            };

            var inMemoryConfigSettings = new Dictionary<string, string> { {"AzureAdSettings", "TestAdSettings"}};

            IConfiguration configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(inMemoryConfigSettings)
                            .Build();

            A.CallTo(() => _fakeCdnService.PurgeContentAsync(A<IList<string>>.Ignored)).Returns(true);
            var mediaBlobStoreEventHandler = new MediaBlobStoreEventHandler(configuration, _fakeCdnService, _logger);

            //act
            await mediaBlobStoreEventHandler.MediaDeletedFileAsync(mediaDeletedContext);

            //assert
            A.CallTo(() => _fakeCdnService.PurgeContentAsync(A<IList<string>>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task MediaDeletedFileAsyncFailedToCallCdnService()
        {
            //arrange
            var mediaDeletedContext = new MediaDeletedContext
            {
                Path = ContentPath,
                Result = true
            };

            var inMemoryConfigSettings = new Dictionary<string, string> ();

            IConfiguration configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(inMemoryConfigSettings)
                            .Build();

            A.CallTo(() => _fakeCdnService.PurgeContentAsync(A<IList<string>>.Ignored)).Returns(true);
            var mediaBlobStoreEventHandler = new MediaBlobStoreEventHandler(configuration, _fakeCdnService, _logger);

            //act
            await mediaBlobStoreEventHandler.MediaDeletedFileAsync(mediaDeletedContext);

            //assert
            A.CallTo(() => _fakeCdnService.PurgeContentAsync(A<IList<string>>.Ignored)).MustNotHaveHappened();
        }
    }
}
