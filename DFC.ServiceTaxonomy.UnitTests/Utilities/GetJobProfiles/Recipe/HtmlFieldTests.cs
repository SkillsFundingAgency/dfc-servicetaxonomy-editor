using System.Collections.Generic;
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

        [Theory]
        [InlineData("<p>You'll spend 2 years as a student officer before becoming a police constable. You'll then decide whether you want to specialise in a particular area of policing. You could consider:<ul><li>Criminal Investigation Department (CID), anti-fraud or road traffic</li><li>drugs or firearms</li><li>counter-terrorism</li><li>air support or underwater search</li><li>dog-handling or mounted policing</li></ul></p><p>With experience you may be able to apply for promotion to sergeant, inspector, chief inspector or higher.</p><p>In the CID you'll also have the title of detective added to your rank - for example, detective sergeant or detective chief inspector.</p>", new[] { "You'll spend 2 years as a student officer before becoming a police constable. You'll then decide whether you want to specialise in a particular area of policing. You could consider:Criminal Investigation Department (CID), anti-fraud or road traffic; drugs or firearms; counter-terrorism; air support or underwater search; dog-handling or mounted policing", "With experience you may be able to apply for promotion to sergeant, inspector, chief inspector or higher.", "In the CID you'll also have the title of detective added to your rank - for example, detective sergeant or detective chief inspector." })]
        [InlineData("<p>Some text with an HTTP <a href=\"http://www.example.com\">link</a> before; some; semi-colons;</p>", new[] { "Some text with an HTTP <a href=\"http://www.example.com\">link</a> before; some; semi-colons;" })]
        [InlineData("<p>Some text with an HTTPS <a href=\"https://www.example.com\">link</a> before; some; semi-colons;</p>", new[] { "Some text with an HTTPS <a href=\"https://www.example.com\">link</a> before; some; semi-colons;" })]
        [InlineData("<p>Some text: with multiple: before a list:<ul><li>of</li><li>some</li><li>stuff</li></ul></p>", new[] { "Some text: with multiple: before a list: of; some; stuff" })]
        [InlineData("<p>Some text: with multiple: and a <a href=\"https://www.example.com\">link</a> before a list:<ul><li>of</li><li>some</li><li>stuff</li></ul></p>", new[] { "Some text: with multiple: and a <a href=\"https://www.example.com\">link</a> before a list: of; some; stuff" })]
        public void HtmlListSubstitutionTests(string expected, IEnumerable<string> source)
        {
            var field = new HtmlField(source);
            Assert.Equal(expected, field.Html);
        }
    }
}
