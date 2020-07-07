using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.LinkFieldGraphSyncerTests
{
    public class LinkFieldGraphSyncer_AddSyncComponentsTests : FieldGraphSyncer_AddSyncComponentsTests
    {
        public LinkFieldGraphSyncer_AddSyncComponentsTests()
        {
            ContentFieldGraphSyncer = new LinkFieldGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_LinkTextInContent_LinkTextAddedToMergeNodeCommandsProperties()
        {
            const string text = "abc";
            string expectedFieldName = $"{_fieldName}_text";

            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
                {{expectedFieldName, text}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullLinkTextInContent_LinkTextNotAddedToMergeNodeCommandsProperties()
        {
            ContentItemField = JObject.Parse("{\"Text\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_LinkUrlInContent_LinkUrlAddedToMergeNodeCommandsProperties()
        {
            const string url = "https://example.com/";
            string expectedFieldName = $"{_fieldName}_url";

            ContentItemField = JObject.Parse($"{{\"Url\": \"{url}\"}}");

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
                {{expectedFieldName, url}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullLinkUrlInContent_LinkUrlNotAddedToMergeNodeCommandsProperties()
        {
            ContentItemField = JObject.Parse("{\"Url\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        //todo: assert that nothing else is done to the commands
        //todo: assert that graphsynchelper's contenttype is not set
    }
}
