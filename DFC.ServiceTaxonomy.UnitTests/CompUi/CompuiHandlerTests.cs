using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using AutoMapper;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Handlers;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Models;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.CompUi
{
    public partial class CompuiHandlerTests
    {
        private readonly IDbConnectionAccessor _fakeDbaAccessor;
        private readonly ILogger<CacheHandler> _fakeLogger;
        private readonly IDapperWrapper _fakeDapperWrapper;
        private readonly ICacheHandler _fakeCacheHandler;
        private readonly ISharedContentRedisInterface _fakeSharedContentRedisInterface;
        private readonly IEventGridHandler _fakeEventHandler;
        public readonly CacheHandler _concreteCacheHander;
        public readonly IMapper _mapper;
        public readonly IBuilder _fakeBuilder;
        public readonly IDirector _fakeDirector;
        public readonly IBackgroundQueue<Processing> _fakeBackgroundQueue;
        private readonly IConfiguration _fakeConfiguration;

        public CompuiHandlerTests()
        {
            _fakeDbaAccessor = A.Fake<IDbConnectionAccessor>();
            _fakeLogger = A.Fake<ILogger<CacheHandler>>();
            _fakeDapperWrapper = A.Fake<IDapperWrapper>();
            _fakeSharedContentRedisInterface = A.Fake<ISharedContentRedisInterface>();
            _fakeBuilder = A.Fake<IBuilder>();
            _mapper = A.Fake<IMapper>();
            _fakeDirector = A.Fake<IDirector>();
            _fakeEventHandler = A.Fake<IEventGridHandler>();
            _fakeBackgroundQueue = A.Fake<IBackgroundQueue<Processing>>();
            _fakeConfiguration = A.Fake<IConfiguration>();
            _fakeCacheHandler = new CacheHandler(_fakeLogger, _mapper, _fakeDirector, _fakeBuilder, _fakeBackgroundQueue, _fakeEventHandler, _fakeConfiguration);
            _concreteCacheHander = new CacheHandler(_fakeLogger, _mapper, _fakeDirector, _fakeBuilder, _fakeBackgroundQueue, _fakeEventHandler, _fakeConfiguration);
        }

        #region Publish Tests       
        [Fact(Skip = "AutoMapper isn't correctly mapping the object and this causes issues further down.  Needs further investigation.  Poss. solution don't fake the Automapper for this call.")]
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
        public async Task PublishNoNodeIdsFoundInDatabase()
        {
            //Arrange 
            var nodeList = new List<NodeItem>();
            var fakeConnection = _fakeDbaAccessor.CreateConnection();

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(fakeConnection, A<string>.Ignored)).Returns(nodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            //var result =
            await _fakeDapperWrapper.QueryAsync<NodeItem>(fakeConnection, "SELECT * FROM TABLE");

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(fakeConnection, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustNotHaveHappened();
        }
        #endregion

        #region Draft Tests
        [Fact (Skip ="AutoMapper isn't correctly mapping the object and this causes issues further down.  Needs further investigation.  Poss. solution don't fake the Automapper for this call.")]
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
