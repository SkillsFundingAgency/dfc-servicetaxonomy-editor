using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using DFC.ServiceTaxonomy.CompUi.Services;
using Moq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.CompUi
{
    public class DirectorTests
    {
        [Fact]
        public async Task ProcessSharedContentAsync_InvokesBuilderMethods()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;
            var processing = new Processing();

            // Act
            await director.ProcessSharedContentAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateAdditionalPageNodesAsync(processing), Times.Once);
            builderMock.Verify(mock => mock.GetContentItemsByLikeQueryAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            //builderMock.Verify(mock => mock.InvalidatePageNodeAsync(It.IsAny<string>(), ProcessingEvents.DraftSaved), Times.AtLeastOnce);
            builderMock.Verify(mock => mock.InvalidateSharedContentAsync(processing), Times.Once);
        }

        [Fact]
        public async Task ProcessWorkingPatternsAsync_InvokesInvalidateDysacJobProfileOverviewRelatedContentItemsAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessWorkingPatternsAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(processing), Times.Once);
        }

        [Fact]
        public async Task ProcessWorkingPatternDetailAsync_InvokesInvalidateDysacJobProfileOverviewRelatedContentItemsAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessWorkingPatternDetailAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(processing), Times.Once);
        }

        [Fact]
        public async Task ProcessWorkingHoursDetailAsync_InvokesInvalidateDysacJobProfileOverviewRelatedContentItemsAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessWorkingHoursDetailAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(processing), Times.Once);
        }
        [Fact]
        public async Task ProcessTriageToolFilterAsync_InvokesInvalidateTriageToolFiltersAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessTriageToolAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateTriageToolFiltersAsync(processing), Times.Once);
        }
        [Fact]
        public async Task ProcessPagebannerAsync_InvokesInvalidatePageBannerAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessPagebannerAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidatePageBannerAsync(processing), Times.Once);
        }
        [Fact]
        public async Task ProcessJobProfileCategoryAsync_InvokesInvalidateJobProfileCategoryAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessJobProfileCategoryAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateJobProfileCategoryAsync(processing), Times.Once);
        }

        [Fact]
        public async Task ProcessJobProfileCategoryAsync_InvokesInvalidateDysacPersonalityTraitAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessJobProfileCategoryAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateDysacPersonalityTraitAsync(processing), Times.Once);
        }

        [Fact]
        public async Task ProcessJobProfileCategoryAsync_InvokesInvalidateDysacJobProfileOverviewRelatedContentItemsAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessJobProfileCategoryAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(processing), Times.Once);
        }

        [Fact]
        public async Task ProcessPersonalityFilteringQuestionAsync_InvokesGetRelatedContentItemIdsAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessPersonalityFilteringQuestionAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.GetRelatedContentItemIdsAsync(processing), Times.Once);
        }

        [Fact]
        public async Task ProcessPersonalityFilteringQuestionAsync_InvokesInvalidateAdditionalContentItemIdsAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessPersonalityFilteringQuestionAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateAdditionalContentItemIdsAsync(processing, It.IsAny<IEnumerable<RelatedItems>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPersonalityFilteringQuestionAsync_InvokesInvalidateDysacPersonalityFilteringQuestionAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessPersonalityFilteringQuestionAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateDysacPersonalityFilteringQuestionAsync(processing), Times.Once);
        }
        [Fact]
        public async Task ProcessPersonalityQuestionSetAsync_InvokesGetRelatedContentItemIdsAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessPersonalityQuestionSetAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.GetRelatedContentItemIdsAsync(processing), Times.Once);
        }

        [Fact]
        public async Task ProcessPersonalityQuestionSetAsync_InvokesInvalidateAdditionalContentItemIdsAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessPersonalityQuestionSetAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateAdditionalContentItemIdsAsync(processing, It.IsAny<IEnumerable<RelatedItems>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPersonalityQuestionSetAsync_InvokesInvalidateDysacPersonalityQuestionSetAsync()
        {
            // Arrange
            var builderMock = new Mock<IBuilder>();
            var director = new Director();
            director.Builder = builderMock.Object;

            var processing = new Processing();

            // Act
            await director.ProcessPersonalityQuestionSetAsync(processing);

            // Assert
            builderMock.Verify(mock => mock.InvalidateDysacPersonalityQuestionSetAsync(processing), Times.Once);
        }


    }
}
