using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.FieldGraphSyncer;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.TextFieldGraphSyncerTests
{
    public class TextFieldGraphSyncer_AddSyncComponentsTests : FieldGraphSyncer_AddSyncComponentsTests
    {
        public TextFieldGraphSyncer_AddSyncComponentsTests()
        {
            ContentFieldGraphSyncer = new TextFieldGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_TextInContent_TextAddedToMergeNodeCommandsProperties()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
                {{_fieldName, text}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullTextInContent_TextNotAddedToMergeNodeCommandsProperties()
        {
            ContentItemField = JObject.Parse("{\"Text\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        //todo: assert that nothing else is done to the commands
        //todo: assert that graphsynchelper's contenttype is not set
    }
}
