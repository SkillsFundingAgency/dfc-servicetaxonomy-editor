using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.DisplayManagement.Notify;
using DFC.ServiceTaxonomy.CompUi.Handlers;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement;
using System.Data.Common;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Common.SharedContent.Pkg.Netcore.IUtilities;

namespace DFC.ServiceTaxonomy.UnitTests.CompUi
{
    public class CompuiHandlerTests
    {
        private readonly IDbConnectionAccessor _fakeDbaAccessor;
        private readonly INotifier _fakeNotifier;
        private readonly IHtmlLocalizer<CacheHandler> _fakeHtmlLocalizer;
        private readonly ILogger<CacheHandler> _fakeLogger;
        private readonly IDapperWrapper _fakeDapperWrapper;
        private readonly ICacheHandler _cacheHandler;
        private readonly ISharedContentRedisInterface _fakeSharedContentRedisInterface;
        private readonly IUtilities _utilities;
        private readonly ContentItem pageContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "Page"
        };
        private readonly ContentItem _sharedContentContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "SharedContent"
        };

        private readonly List<NodeItem> _emptyNodeList = new List<NodeItem>();
        private readonly List<NodeItem> _oneItemPagenodeList = new List<NodeItem>
            {
                new NodeItem()
                {
                    NodeId = "<<contentapiprefix>>/sharedcontent/bfee0a93-cd0d-40eb-a72d-7bb0c0cced3e",
                    Content = "{\r\n  \"ContentItemId\": \"4yhev1xmryp1mvjkd9693kwqc9\",\r\n  \"ContentItemVersionId\": \"4d320x99yhatnws0yxdwx05bdm\",\r\n  \"ContentType\": \"Page\",\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"send-us-a-letter\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"/contact-us/send\\r\\n/contact-us/letter\",\r\n    \"FullUrl\": \"/contact-us/send-us-a-letter\"\r\n  },\r\n}"}
            };
        private readonly List<NodeItem> _twoItemPageNodeList = new List<NodeItem>
            {
                new NodeItem() {
                    NodeId = "<<contentapiprefix>>/sharedcontent/bfee0a93-cd0d-40eb-a72d-7bb0c0cced3e",
                    Content = "{\r\n  \"ContentItemId\": \"4yhev1xmryp1mvjkd9693kwqc9\",\r\n  \"ContentItemVersionId\": \"4d320x99yhatnws0yxdwx05bdm\",\r\n  \"ContentType\": \"Page\",\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"send-us-a-letter\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"/contact-us/send\\r\\n/contact-us/letter\",\r\n    \"FullUrl\": \"/contact-us/send-us-a-letter\"\r\n  },\r\n}"},
                new NodeItem() {
                    NodeId = "<<contentapiprefix>>/sharedcontent/bfee0a93-cd0d-40eb-a72d-7bb0c0cced3e",
                    Content = "{\r\n  \"ContentItemId\": \"4yhev1xmryp1mvjkd9693kwqc9\",\r\n  \"ContentItemVersionId\": \"4d320x99yhatnws0yxdwx05bdm\",\r\n  \"ContentType\": \"Page\",\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"send-us-a-letter\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"/contact-us/send\\r\\n/contact-us/letter\",\r\n    \"FullUrl\": \"/contact-us/send-us-a-letter\"\r\n  },\r\n}"}
            };


        public CompuiHandlerTests()
        {
            _fakeDbaAccessor = A.Fake<IDbConnectionAccessor>();
            _fakeNotifier = A.Fake<INotifier>();
            _fakeHtmlLocalizer = A.Fake<IHtmlLocalizer<CacheHandler>>();
            _fakeLogger = A.Fake<ILogger<CacheHandler>>();
            _fakeDapperWrapper = A.Fake<IDapperWrapper>();
            _fakeSharedContentRedisInterface = A.Fake<ISharedContentRedisInterface>();
            _utilities = new Utilities();
            _cacheHandler = new CacheHandler(
                _fakeDbaAccessor,
                _fakeNotifier,
                _fakeHtmlLocalizer,
                _fakeLogger,
                _fakeDapperWrapper,
                _fakeSharedContentRedisInterface,
                _utilities);
        }

        #region Publish Tests
        [Fact]
        public async Task PublishSingleNodeIdAffected()
        {
            //Arrange 
            var _publishContentContext = new PublishContentContext(_sharedContentContentitem, _sharedContentContentitem);
            var nodeList = new List<NodeItem>
            {
                new NodeItem() { NodeId = "<<contentapiprefix>>/sharedcontent/bfee0a93-cd0d-40eb-a72d-7bb0c0cced3e" }
            };

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(nodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _cacheHandler.ProcessPublishedAsync(_publishContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PublishTwoNodeIdsAffected()
        {
            //Arrange 
            var _publishContentContext = new PublishContentContext(_sharedContentContentitem, _sharedContentContentitem);
            var nodeList = new List<NodeItem>
            {
                new NodeItem() { NodeId = "<<contentapiprefix>>/sharedcontent/bfee0a93-cd0d-40eb-a72d-7bb0c0cced3e" },
                new NodeItem() { NodeId = "<<contentapiprefix>>/sharedcontent/bfee0a93-cd0d-40eb-a72d-7bb0c0cced3e" }
            };

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(nodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _cacheHandler.ProcessPublishedAsync(_publishContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task PublishNoNodeIdsFoundInDatabase()
        {
            //Arrange 
            var nodeList = new List<NodeItem>();
            var fakeConnection = _fakeDbaAccessor.CreateConnection();

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(fakeConnection, A<string>.Ignored)).Returns(nodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            //var result =
            await _fakeDapperWrapper.QueryAsync<NodeItem>(fakeConnection, "SELECT * FROM TABLE").ConfigureAwait(false);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(fakeConnection, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustNotHaveHappened();
        }
        #endregion

        #region Draft Tests
        [Fact]
        public async Task DraftSingleNodeIdAffected()
        {
            //Arrange 
            var _saveDraftContentContext = new SaveDraftContentContext(pageContentitem);

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_oneItemPagenodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _cacheHandler.ProcessDraftSavedAsync(_saveDraftContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task DraftTwoNodeIdsAffected()
        {
            //Arrange
            var _saveDraftContentContext = new SaveDraftContentContext(pageContentitem);

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_twoItemPageNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _cacheHandler.ProcessDraftSavedAsync(_saveDraftContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task DraftNoNodeIdsFoundInDatabase()
        {
            //Arrange
            var _saveDraftContentContext = new SaveDraftContentContext(pageContentitem);
            

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_emptyNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _cacheHandler.ProcessDraftSavedAsync(_saveDraftContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustNotHaveHappened();
        }

        #endregion
    }
}
