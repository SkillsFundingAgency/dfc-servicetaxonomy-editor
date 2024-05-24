using Xunit;
using Moq;
using DFC.ServiceTaxonomy.CompUi.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using OrchardCore.Data;
using System.Data.Common;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Models;
using DFC.ServiceTaxonomy.CompUi.AppRegistry;

namespace DFC.ServiceTaxonomy.UnitTests.CompUi
{
    public class ConcreteBuilderTests
    {
        [Fact]
        public async Task GetDataAsync_ReturnsData()
        {

            // Arrange
            var dbaAccessorMock = new Mock<IDbConnectionAccessor>();
            var dapperWrapperMock = new Mock<IDapperWrapper>();
            var sharedContentRedisInterfaceMock = new Mock<ISharedContentRedisInterface>();
            var loggerMock = new Mock<ILogger<ConcreteBuilder>>();
            var pageLocationUpdater = new Mock<IPageLocationUpdater>();

            var expectedData = new List<NodeItem> { new NodeItem { NodeId = "1", Content = "Data" } };
            dbaAccessorMock.Setup(mock => mock.CreateConnection()).Returns(Mock.Of<DbConnection>());
            dapperWrapperMock.Setup(mock => mock.QueryAsync<NodeItem>(It.IsAny<DbConnection>(), It.IsAny<string>())).ReturnsAsync(expectedData);
1.1
            var concreteBuilder = new ConcreteBuilder(dbaAccessorMock.Object, dapperWrapperMock.Object, sharedContentRedisInterfaceMock.Object, loggerMock.Object, pageLocationUpdater.Object);

            // Act
            var result = await concreteBuilder.GetDataAsync(new Processing());

            // Assert
            Assert.Equal(expectedData, result);
        }

        [Fact]
        public async Task GetRelatedContentItemIdsAsync_ReturnsData()
        {
            // Arrange
            var dbaAccessorMock = new Mock<IDbConnectionAccessor>();
            var dapperWrapperMock = new Mock<IDapperWrapper>();
            var sharedContentRedisInterfaceMock = new Mock<ISharedContentRedisInterface>();
            var loggerMock = new Mock<ILogger<ConcreteBuilder>>();
            var pageLocationUpdater = new Mock<IPageLocationUpdater>();

            var expectedData = new List<RelatedItems> { new RelatedItems { ContentItemId = "1", ContentType = "SharedContent" } };
            dbaAccessorMock.Setup(mock => mock.CreateConnection()).Returns(Mock.Of<DbConnection>());
            dapperWrapperMock.Setup(mock => mock.QueryAsync<string>(It.IsAny<DbConnection>(), It.IsAny<string>())).ReturnsAsync(new List<string> { "1" });
            dapperWrapperMock.Setup(mock => mock.QueryAsync<RelatedItems>(It.IsAny<DbConnection>(), It.IsAny<string>())).ReturnsAsync(expectedData);

            var concreteBuilder = new ConcreteBuilder(dbaAccessorMock.Object, dapperWrapperMock.Object, sharedContentRedisInterfaceMock.Object, loggerMock.Object, pageLocationUpdater.Object);

            // Act
            var result = await concreteBuilder.GetRelatedContentItemIdsAsync(new Processing());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedData, result!);
        }
    }
}
