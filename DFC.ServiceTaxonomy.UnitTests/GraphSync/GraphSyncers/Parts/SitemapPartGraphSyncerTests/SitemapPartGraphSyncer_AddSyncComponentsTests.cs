using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.PartGraphSyncer;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.SitemapPartGraphSyncerTests
{
    public class SitemapPartGraphSyncer_AddSyncComponentsTests : PartGraphSyncer_AddSyncComponentsTests
    {
        public SitemapPartGraphSyncer_AddSyncComponentsTests()
        {
            A.CallTo(() => GraphSyncHelper.PropertyName("OverrideSitemapConfig")).Returns("sitemap_OverrideSitemapConfig");
            A.CallTo(() => GraphSyncHelper.PropertyName("ChangeFrequency")).Returns("sitemap_ChangeFrequency");
            A.CallTo(() => GraphSyncHelper.PropertyName("Priority")).Returns("sitemap_Priority");
            A.CallTo(() => GraphSyncHelper.PropertyName("Exclude")).Returns("sitemap_Exclude");

            ContentPartGraphSyncer = new SitemapPartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_AllPropertiesInContent_AllPropertiesAddedToMergeNodeCommandsProperties()
        {
            Content = JObject.Parse(@$"{{""OverrideSitemapConfig"": ""{true.ToString().ToLower()}"",
                                        ""ChangeFrequency"": 2, ""Priority"": 1, ""Exclude"": false}}");

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
            {
                {"sitemap_OverrideSitemapConfig", true},
                {"sitemap_ChangeFrequency", "weekly"},
                {"sitemap_Priority", 1},
                {"sitemap_Exclude", false}
            };

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        //todo: split out tests
    }
}
