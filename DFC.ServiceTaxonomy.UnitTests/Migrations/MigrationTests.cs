using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Migration.Migrations;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Migrations
{
    public class MigrationTests
    {
        private readonly Mock<IRecipeMigrator> recipeMigrator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<RecipeMigration>> logger = new();

        [Fact]
        public async Task CreateAsync_ReturnsExpected()
        {
            //Arrange
            recipeMigrator.Setup(service => service.ExecuteAsync(It.IsAny<string>(), It.IsAny<IDataMigration>())).ReturnsAsync(string.Empty);
            var migration = new RecipeMigration(recipeMigrator.Object, logger.Object);

            //Act
            var result = await migration.CreateAsync();

            //Assert
            recipeMigrator.Verify(service => service.ExecuteAsync(It.IsAny<string>(), It.IsAny<IDataMigration>()), Times.Exactly(2));
            result.Should().Be(1);
        }
    }
}

