using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.PartGraphSyncer;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.HtmlBodyPartGraphSyncerTests
{
    public class HtmlBodyPartGraphSyncer_AddSyncComponentsTests : PartGraphSyncer_AddSyncComponentsTests
    {
        public HtmlBodyPartGraphSyncer_AddSyncComponentsTests()
        {
            ContentPartGraphSyncer = new HtmlBodyPartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_HtmlInContent_TitleAddedToMergeNodeCommandsProperties()
        {
            A.CallTo(() => GraphSyncHelper.PropertyName("Html")).Returns("htmlbody_Html");

            const string html = "<p>A test paragraph</p>";

            Content = JObject.Parse($"{{\"Html\": \"{html}\"}}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>
                {{"htmlbody_Html", html}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullHtmlInContent_TitleNotAddedToMergeNodeCommandsProperties()
        {
            A.CallTo(() => GraphSyncHelper.PropertyName("Html")).Returns("htmlbody_Html");

            Content = JObject.Parse("{\"Html\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }
    }
}
