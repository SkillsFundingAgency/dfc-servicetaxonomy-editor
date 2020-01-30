using GetJobProfiles.Models.Recipe;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Utilities.GetJobProfiles.Recipe
{
    public class HtmlFieldTests
    {
        //todo: fix commented out tests (only really required if examples of [ & ] in sitefinity data)
        [Theory]
        [InlineData("<p></p>", "")]

        [InlineData("<p><a href=\"https://example.com/\">Text</a></p>", "[Text | https://example.com/]")]

        [InlineData("<p>Abc<a href=\"https://example.com/\">Text</a>Xyz</p>", "Abc[Text | https://example.com/]Xyz")]
        [InlineData("<p>Abc <a href=\"https://example.com/\">Text</a> Xyz</p>", "Abc [Text | https://example.com/] Xyz")]
        // [InlineData("<p>[] <a href=\"https://example.com/\">Text</a> []</p>", "[] [Text | https://example.com/] []")]
        [InlineData("<p>| <a href=\"https://example.com/\">Text</a> |</p>", "| [Text | https://example.com/] |")]
        //[InlineData("<p>| <a href=\"https://example.com/\">Text</a> []</p>", "[] [Text | https://example.com/] []")]
        // [InlineData("<p>[] <a href=\"https://example.com/\">Text</a> |</p>", "| [Text | https://example.com/] |")]
        // [InlineData("<p>|[]|] <a href=\"https://example.com/\">Text</a> |</p>", "| [Text | https://example.com/] ||][||")]

        [InlineData("<p><a href=\"https://example.com/\">Text</a><a href=\"https://example2.com/\">Text2</a></p>", "[Text | https://example.com/][Text2 | https://example2.com/]")]
        [InlineData("<p>Abc<a href=\"https://example.com/\">Text</a>Lmn<a href=\"https://example2.com/\">Text2</a>Xyz</p>", "Abc[Text | https://example.com/]Lmn[Text2 | https://example2.com/]Xyz")]
        [InlineData("<p>Abc <a href=\"https://example.com/\">Text</a> Lmn <a href=\"https://example2.com/\">Text2</a> Xyz</p>", "Abc [Text | https://example.com/] Lmn [Text2 | https://example2.com/] Xyz")]
        // [InlineData("<p>[] <a href=\"https://example.com/\">Text</a> [] <a href=\"https://example2.com/\">Text2</a> []</p>", "[] [Text | https://example.com/] [] [Text2 | https://example2.com/] []")]
        [InlineData("<p>| <a href=\"https://example.com/\">Text</a> | <a href=\"https://example2.com/\">Text2</a> |</p>", "| [Text | https://example.com/] | [Text2 | https://example2.com/] |")]

        [InlineData("<p>You can find out more about becoming an acoustics consultant from the <a href=\"https://www.ioa.org.uk/careers\">Institute of Acoustics</a>.</p>", "You can find out more about becoming an acoustics consultant from the [Institute of Acoustics | https://www.ioa.org.uk/careers].")]
        [InlineData(@"<p>You can find out more about a career in advertising from the <a href=""https://ipa.co.uk/knowledge/careers-in-advertising/"">Institute of Practitioners in Advertising</a>.</p>", @"You can find out more about a career in advertising from the [Institute of Practitioners in Advertising | https://ipa.co.uk/knowledge/careers-in-advertising/
].")]
        public void HtmlLinkSubstitutionTests(string expected, string source)
        {
            var htmlField = new HtmlField(source);
            Assert.Equal(expected, htmlField.Html);
        }
    }
}
