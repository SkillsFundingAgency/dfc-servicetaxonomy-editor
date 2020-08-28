using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.HtmlFieldGraphSyncerTests
{
    public class HtmlFieldGraphSyncerAddSyncComponentsTestsBase : FieldGraphSyncer_AddSyncComponentsTestsBase
    {
        public HtmlFieldGraphSyncerAddSyncComponentsTestsBase()
        {
            ContentFieldGraphSyncer = new HtmlFieldGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_HtmlInContent_HtmlAddedToMergeNodeCommandsProperties()
        {
            const string html = "<p>abc</p>";

            ContentItemField = JObject.Parse($"{{\"Html\": \"{html}\"}}");

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
                {{_fieldName, html}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullHtmlInContent_HtmlNotAddedToMergeNodeCommandsProperties()
        {
            ContentItemField = JObject.Parse("{\"Html\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        //todo: assert that nothing else is done to the commands
        //todo: assert that syncnameprovider's contenttype is not set
    }
}
