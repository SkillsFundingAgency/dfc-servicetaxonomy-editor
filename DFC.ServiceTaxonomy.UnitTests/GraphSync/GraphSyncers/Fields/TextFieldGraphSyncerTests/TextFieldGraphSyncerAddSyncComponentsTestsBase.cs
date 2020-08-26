using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.TextFieldGraphSyncerTests
{
    public class TextFieldGraphSyncerAddSyncComponentsTestsBase : FieldGraphSyncer_AddSyncComponentsTestsBase
    {
        public TextFieldGraphSyncerAddSyncComponentsTestsBase()
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
        //todo: assert that syncnameprovider's contenttype is not set
    }
}
