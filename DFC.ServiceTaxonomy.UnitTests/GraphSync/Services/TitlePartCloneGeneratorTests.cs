using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Services;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Services
{
    public class TitlePartCloneGeneratorTests
    {
        [Theory]
        [InlineData("", "CLONE - ")]
        [InlineData("National Careers Service", "CLONE - National Careers Service")]
        [InlineData("CLONE - National Careers Service", "CLONE (2) - National Careers Service")]
        [InlineData("CLONE (2) - National Careers Service", "CLONE (3) - National Careers Service")]
        public Task Cloning_GeneratesCorrectPropertiesWithPreviousClones(string title, string expectedTitle)
        {
            var generator = new TitlePartCloneGenerator();

            var result = generator.Generate(title);

            Assert.Equal(expectedTitle, result);

            return Task.CompletedTask;
        }
    }
}
