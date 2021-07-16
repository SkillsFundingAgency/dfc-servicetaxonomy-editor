using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Media.Events;
using DFC.ServiceTaxonomy.Media.Services;
using FakeItEasy;
using OrchardCore.Media.Events;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Media.Events
{
    public  class MediaBlobStoreEventHandlerTests
    {
        private readonly ICdnService FakeCdnService;

        public MediaBlobStoreEventHandlerTests()
        {
            FakeCdnService = A.Fake<ICdnService>();
        }

        [Fact]
        public async Task MediaDeletedFileAsyncSuccess()
        {
            //arrange
            string? contentPath = "/media/testImage.png";
            var mediaDeletedContext = new MediaDeletedContext
            {
                Path = contentPath,
                Result = true
            };

            A.CallTo(() => FakeCdnService.PurgeContentAsync(A<IList<string>>.Ignored)).Returns(true);
            var mediaBlobStoreEventHandler = new MediaBlobStoreEventHandler(FakeCdnService);

            //act
            await mediaBlobStoreEventHandler.MediaDeletedFileAsync(mediaDeletedContext);

            //assert
            A.CallTo(() => FakeCdnService.PurgeContentAsync(A<IList<string>>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
