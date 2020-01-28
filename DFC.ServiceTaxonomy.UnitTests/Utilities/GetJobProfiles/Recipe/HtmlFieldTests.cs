using GetJobProfiles.Models.Recipe;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Utilities.GetJobProfiles.Recipe
{
    public class HtmlFieldTests
    {
        //todo: fix commented out tests (only really required if examples of [ & ] in sitefinity data)
        [Theory]
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
        public void LinkSubstitutionTests(string expected, string source)
        {
            var htmlField = new HtmlField(source);
            Assert.Equal(expected, htmlField.Html);
        }
    }
}
