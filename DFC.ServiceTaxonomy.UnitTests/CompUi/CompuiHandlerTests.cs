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
using DFC.ServiceTaxonomy.CompUi.Enums;

namespace DFC.ServiceTaxonomy.UnitTests.CompUi
{
    public class CompuiHandlerTests
    {
        private readonly IDbConnectionAccessor _fakeDbaAccessor;
        private readonly INotifier _fakeNotifier;
        private readonly IHtmlLocalizer<CacheHandler> _fakeHtmlLocalizer;
        private readonly ILogger<CacheHandler> _fakeLogger;
        private readonly IDapperWrapper _fakeDapperWrapper;
        private readonly ICacheHandler _fakeCacheHandler;
        private readonly ISharedContentRedisInterface _fakeSharedContentRedisInterface;
        private readonly IUtilities _utilities;
        public readonly CacheHandler _concreteCacheHander;

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
        private readonly ContentItem _jobProfileCategoryContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "JobProfileCategory"
        };
        private readonly ContentItem _jobProfileContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "JobProfile"
        };
        private readonly ContentItem _bannerContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "Banner"
        };
        private readonly ContentItem _pagebannerContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "Pagebanner"
        };
        private readonly ContentItem _socCodeContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "SocCode"
        };

        private readonly List<NodeItem> _emptyNodeList = new List<NodeItem>();

        private readonly NodeItem _pageNode = new NodeItem()
        {
            NodeId = "<<contentapiprefix>>/page/f680eadb-be22-4f4d-bfc2-c6c82ad04981",
            Content = "{\"ContentItemId\":\"4yhev1xmryp1mvjkd9693kwqc9\",\"ContentItemVersionId\":\"4d320x99yhatnws0yxdwx05bdm\",\"ContentType\":\"Page\",\"DisplayText\":\"Send us a letter x\",\"Latest\":true,\"Published\":false,\"ModifiedUtc\":\"2024-01-22T17:47:15.0351653Z\",\"PublishedUtc\":\"2024-01-16T22:06:13.8672169Z\",\"CreatedUtc\":\"2020-07-24T10:59:54.3235574Z\",\"Owner\":\"StaxEditorAdmin\",\"Author\":\"Gavin\",\"TitlePart\":{\"Title\":\"Send us a letter x\"},\"Page\":{\"PageLocations\":{\"TaxonomyContentItemId\":\"4eembshqzx66drajtdten34tc8\",\"TermContentItemIds\":[\"4pksnz9106ngbwq74w66snan5x\"]},\"Description\":{\"Text\":\"Send us a letter page\"},\"Herobanner\":{\"Html\":\"\"},\"ShowHeroBanner\":{\"Value\":false},\"ShowBreadcrumb\":{\"Value\":false},\"TriageToolSummary\":{\"Html\":\"\"},\"UseInTriageTool\":{\"Value\":false},\"TriageToolFilters\":{\"ContentItemIds\":[]}},\"SitemapPart\":{\"Priority\":5,\"OverrideSitemapConfig\":false,\"ChangeFrequency\":0,\"Exclude\":false},\"FlowPart\":{\"Widgets\":[{\"ContentItemId\":\"4n6fn5zwwp06jsqtypnwn1x7wz\",\"ContentItemVersionId\":null,\"ContentType\":\"HTML\",\"DisplayText\":\"\",\"Latest\":false,\"Published\":false,\"ModifiedUtc\":\"2024-01-22T17:47:15.4607733Z\",\"PublishedUtc\":null,\"CreatedUtc\":null,\"Owner\":\"\",\"Author\":\"Gavin\",\"HTML\":{},\"HtmlBodyPart\":{\"Html\":\"<h1>Send us a letter</h1><p>Please post your letter to:</p><p>National Careers Service<br>PO Box 1331<br>Newcastle Upon Tyne<br>NE99 5EB</p><p>Please allow up to 14 days for a response to your letter. If you would prefer us to respond by phone or email, please include those details in your letter.</p><p>If you would like a response sooner, you can <a href=\\\"/contact-us\\\" class=\\\"govuk-link\\\">contact us online</a></p><p><a href=\\\"/contact-us\\\" class=\\\"govuk-button\\\" data-module=\\\"govuk-button\\\">Back</a></p>\"},\"GraphSyncPart\":{\"Text\":\"<<contentapiprefix>>/html/9fa92779-0a72-430d-a2a9-16d1c0b24f99\"},\"FlowMetadata\":{\"Alignment\":0,\"Size\":100}}]},\"GraphSyncPart\":{\"Text\":\"<<contentapiprefix>>/page/f680eadb-be22-4f4d-bfc2-c6c82ad04981\"},\"PageLocationPart\":{\"UrlName\":\"send-us-a-letter\",\"DefaultPageForLocation\":false,\"RedirectLocations\":\"/contact-us/send\\r\\n/contact-us/letter\",\"FullUrl\":\"/contact-us/send-us-a-letter\"},\"UnpublishLaterPart\":{\"ScheduledUnpublishUtc\":null},\"ContentApprovalPart\":{\"ReviewStatus\":0,\"ReviewType\":0,\"IsForcePublished\":true},\"AuditTrailPart\":{\"Comment\":null,\"ShowComment\":false}}"
        };

        private readonly List<NodeItem> _oneItemPageNodeList = new List<NodeItem>
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
            _fakeCacheHandler = new CacheHandler(
                _fakeDbaAccessor,
                _fakeNotifier,
                _fakeHtmlLocalizer,
                _fakeLogger,
                _fakeDapperWrapper,
                _fakeSharedContentRedisInterface,
                _utilities);
            _concreteCacheHander = new CacheHandler(_fakeDbaAccessor,
                _fakeNotifier,
                _fakeHtmlLocalizer,
                _fakeLogger,
                _fakeDapperWrapper,
                _fakeSharedContentRedisInterface,
                _utilities);
        }

        #region Publish Tests
        [Fact]
        public async Task EnsureOnlySpecifiedPublishContentTypesAreProcess()
        {
            //Arrange 
            var _publishContentContext = new PublishContentContext(_socCodeContentitem, _socCodeContentitem);

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_oneItemPageNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _fakeCacheHandler.ProcessPublishedAsync(_publishContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task PublishSingleNodeIdAffected()
        {
            //Arrange 
            var _publishContentContext = new PublishContentContext(_sharedContentContentitem, _sharedContentContentitem);

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_oneItemPageNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _fakeCacheHandler.ProcessPublishedAsync(_publishContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PublishTwoNodeIdsAffected()
        {
            //Arrange 
            var _publishContentContext = new PublishContentContext(_sharedContentContentitem, _sharedContentContentitem);

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_twoItemPageNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _fakeCacheHandler.ProcessPublishedAsync(_publishContentContext);

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

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_oneItemPageNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _fakeCacheHandler.ProcessDraftSavedAsync(_saveDraftContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustHaveHappened(3, Times.Exactly);
        }

        [Fact]
        public async Task DraftTwoNodeIdsAffected()
        {
            //Arrange
            var _saveDraftContentContext = new SaveDraftContentContext(pageContentitem);

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_twoItemPageNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _fakeCacheHandler.ProcessDraftSavedAsync(_saveDraftContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustHaveHappened(6, Times.Exactly);
        }

        [Fact]
        public async Task DraftNoNodeIdsFoundInDatabase()
        {
            //Arrange
            var _saveDraftContentContext = new SaveDraftContentContext(pageContentitem);


            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_emptyNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _fakeCacheHandler.ProcessDraftSavedAsync(_saveDraftContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task EnsureOnlySpecifiedDraftContentTypesAreProcess()
        {
            //Arrange
            var _saveDraftContentContext = new SaveDraftContentContext(_socCodeContentitem);

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_emptyNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _fakeCacheHandler.ProcessDraftSavedAsync(_saveDraftContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustNotHaveHappened();
        }

        #endregion

        #region NodeFormats
        [Fact]
        public void CheckSharedContentNodeFormatIsCorrect()
        {
            //Arrange


            //Act 
            var result = _concreteCacheHander.ResolvePublishNodeId(_pageNode, PublishedContentTypes.Page.ToString());

            //Assert
            Assert.Equal("Page/contact-us/send-us-a-letter/PUBLISHED", result);
        }

        #endregion
    }
}
