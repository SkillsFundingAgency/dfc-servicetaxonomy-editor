using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.HtmlFieldGraphSyncerTests
{
    //todo: factor out common code in tests
    public class HtmlFieldGraphSyncer_AddSyncComponentsTests
    {
        public JObject? ContentItemField { get; set; }
        public IContentManager ContentManager { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public HtmlFieldGraphSyncer HtmlFieldGraphSyncer { get; set; }

        const string _fieldName = "TestField";

        public HtmlFieldGraphSyncer_AddSyncComponentsTests()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();

            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);

            ContentManager = A.Fake<IContentManager>();

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);

            HtmlFieldGraphSyncer = new HtmlFieldGraphSyncer();
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
        //todo: assert that graphsynchelper's contenttype is not set

        private async Task CallAddSyncComponents()
        {
            await HtmlFieldGraphSyncer.AddSyncComponents(
                ContentItemField!,
                ContentManager,
                MergeNodeCommand,
                ReplaceRelationshipsCommand,
                ContentPartFieldDefinition,
                GraphSyncHelper);
        }
    }
}
