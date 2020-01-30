using GetJobProfiles.Models.Recipe;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Utilities.GetJobProfiles.Recipe
{
    public class TitlePartTests
    {
        [Theory]
        [InlineData("", "")]

        [InlineData("[Text]", "[Text | https://example.com/]")]

        [InlineData("Abc[Text]Xyz", "Abc[Text | https://example.com/]Xyz")]
        [InlineData("Abc [Text] Xyz", "Abc [Text | https://example.com/] Xyz")]
        // [InlineData("[] [Text] []", "[] [Text | https://example.com/] []")]
        [InlineData("| [Text] |", "| [Text | https://example.com/] |")]
        //[InlineData("| [Text] []", "[] [Text | https://example.com/] []")]
        // [InlineData("[] [Text] |", "| [Text | https://example.com/] |")]
        // [InlineData("|[]|] [Text] |", "| [Text | https://example.com/] ||][||")]

        [InlineData("[Text][Text2]", "[Text | https://example.com/][Text2 | https://example2.com/]")]
        [InlineData("Abc[Text]Lmn[Text2]Xyz", "Abc[Text | https://example.com/]Lmn[Text2 | https://example2.com/]Xyz")]
        [InlineData("Abc [Text] Lmn [Text2] Xyz", "Abc [Text | https://example.com/] Lmn [Text2 | https://example2.com/] Xyz")]
        // [InlineData("[] [Text] [] [Text2] []", "[] [Text | https://example.com/] [] [Text2 | https://example2.com/] []")]
        [InlineData("| [Text] | [Text2] |", "| [Text | https://example.com/] | [Text2 | https://example2.com/] |")]

        [InlineData("You can find out more about becoming an acoustics consultant from the [Institute of Acoustics].", "You can find out more about becoming an acoustics consultant from the [Institute of Acoustics | https://www.ioa.org.uk/careers].")]
        [InlineData(@"You can find out more about a career in advertising from the [Institute of Practitioners in Advertising].", @"You can find out more about a career in advertising from the [Institute of Practitioners in Advertising | https://ipa.co.uk/knowledge/careers-in-advertising/
].")]
        public void TitleLinkSubstitutionTests(string expected, string source)
        {
            var titlePart = new TitlePart(source);
            Assert.Equal(expected, titlePart.Title);
        }
    }
}
