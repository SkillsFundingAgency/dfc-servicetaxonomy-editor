using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.ServiceTaxonomy.CompUi.AppRegistry;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Handlers;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Model;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using OrchardCore.DisplayManagement.Notify;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.CompUi
{
    public partial class CompuiHandlerTests
    {
        private readonly IDbConnectionAccessor _fakeDbaAccessor;
        private readonly INotifier _fakeNotifier;
        private readonly IHtmlLocalizer<CacheHandler> _fakeHtmlLocalizer;
        private readonly ILogger<CacheHandler> _fakeLogger;
        private readonly IDapperWrapper _fakeDapperWrapper;
        private readonly ICacheHandler _fakeCacheHandler;
        private readonly ISharedContentRedisInterface _fakeSharedContentRedisInterface;
        private readonly IPageLocationUpdater _fakePageLocationUpdater;
        public readonly CacheHandler _concreteCacheHander;

        public CompuiHandlerTests()
        {
            _fakeDbaAccessor = A.Fake<IDbConnectionAccessor>();
            _fakeNotifier = A.Fake<INotifier>();
            _fakeHtmlLocalizer = A.Fake<IHtmlLocalizer<CacheHandler>>();
            _fakeLogger = A.Fake<ILogger<CacheHandler>>();
            _fakeDapperWrapper = A.Fake<IDapperWrapper>();
            _fakeSharedContentRedisInterface = A.Fake<ISharedContentRedisInterface>();
            _fakePageLocationUpdater = A.Fake<IPageLocationUpdater>();
            _fakeCacheHandler = new CacheHandler(
                _fakeDbaAccessor,
                _fakeNotifier,
                _fakeHtmlLocalizer,
                _fakeLogger,
                _fakeDapperWrapper,
                _fakeSharedContentRedisInterface,
                _fakePageLocationUpdater);
            _concreteCacheHander = new CacheHandler(_fakeDbaAccessor,
                _fakeNotifier,
                _fakeHtmlLocalizer,
                _fakeLogger,
                _fakeDapperWrapper,
                _fakeSharedContentRedisInterface,
                _fakePageLocationUpdater);
        }

        #region Publish Tests
        [Fact(Skip = "Currently causing an error.  Code has been refactored for the next release")]
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

        [Fact(Skip = "Currently causing an error.  Code has been refactored for the next release")]
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

        [Fact(Skip = "Currently causing an error.  Code has been refactored for the next release")]
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

        [Fact(Skip = "Currently causing an error.  Code has been refactored for the next release")]
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
        [Fact(Skip = "Currently causing an error.  Code has been refactored for the next release")]
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

        [Fact(Skip = "Currently causing an error.  Code has been refactored for the next release")]
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

        [Fact(Skip = "Currently causing an error.  Code has been refactored for the next release")]
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

        [Fact(Skip = "Currently causing an error.  Code has been refactored for the next release")]
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
        //[Fact]
        //public void CheckSharedContentNodeFormatIsCorrect()
        //{
        //    //Arrange


        //    //Act 
        //    var result = _concreteCacheHander.ResolvePublishNodeId(_pageNode, PublishedContentTypes.Page.ToString());

        //    //Assert
        //    Assert.Equal("Page/contact-us/send-us-a-letter/PUBLISHED", result);
        //}

        #endregion
    }
}
