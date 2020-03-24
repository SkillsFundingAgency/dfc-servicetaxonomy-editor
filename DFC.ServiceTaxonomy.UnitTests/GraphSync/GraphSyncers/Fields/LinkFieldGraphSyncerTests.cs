using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields
{
    public class LinkFieldGraphSyncerTests
    {
        public JObject? ContentItemField { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public LinkFieldGraphSyncer LinkFieldGraphSyncer { get; set; }

        const string _fieldName = "TestField";

        public LinkFieldGraphSyncerTests()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();

            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);

            LinkFieldGraphSyncer = new LinkFieldGraphSyncer();
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

        private async Task CallAddSyncComponents()
        {
            await LinkFieldGraphSyncer.AddSyncComponents(
                ContentItemField!,
                MergeNodeCommand,
                ReplaceRelationshipsCommand,
                ContentPartFieldDefinition,
                GraphSyncHelper);
        }
    }
}
