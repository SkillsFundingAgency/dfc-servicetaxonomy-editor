using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Models;
using DFC.ServiceTaxonomy.PageLocation.Services;
using OrchardCore.ContentManagement;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.PageLocation.Services
{
    public class PageLocationClonePropertyGeneratorTests
    {
        [Theory]
        [InlineData("/home")]
        [InlineData("/home-clone")]
        [InlineData("/home-clone-2")]
        public Task Cloning_GeneratesCorrectUrlSearchFragment(string url)
        {
            var generator = new PageLocationClonePropertyGenerator();

            var result = generator.GenerateUrlSearchFragment(url);

            Assert.Equal("/home-clone", result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task Cloning_GeneratesCorrectPropertiesWithNoPreviousClones()
        {
            var generator = new PageLocationClonePropertyGenerator();

            var result = generator.GenerateClonedPageLocationProperties("home", "/home", new List<ContentItem>());

            Assert.Equal("home-clone", result.UrlName);
            Assert.Equal("/home-clone", result.FullUrl);

            return Task.CompletedTask;
        }

        [Theory]
        [InlineData("/home-clone", "home", "/home", "home-clone-2", "/home-clone-2")]
        [InlineData("/home-clone-2", "home", "/home", "home-clone-3", "/home-clone-3")]
        [InlineData("/home-clone", "home-clone", "/home-clone", "home-clone-2", "/home-clone-2")]
        [InlineData("/home-clone-2", "home-clone", "/home-clone", "home-clone-3", "/home-clone-3")]
        public Task Cloning_GeneratesCorrectPropertiesWithPreviousClones(string lastCloneUrl, string urlName, string fullUrl, string expectedUrlName, string expectedFullUrl)
        {
            var contentItem = new ContentItem();
            contentItem.CreatedUtc = DateTime.Now;
            contentItem.GetOrCreate<PageLocationPart>();
            contentItem.Content.PageLocationPart.FullUrl = lastCloneUrl;

            var generator = new PageLocationClonePropertyGenerator();

            var result = generator.GenerateClonedPageLocationProperties(urlName, fullUrl, new List<ContentItem> { contentItem });

            Assert.Equal(expectedUrlName, result.UrlName);
            Assert.Equal(expectedFullUrl, result.FullUrl);

            return Task.CompletedTask;
        }
    }
}
