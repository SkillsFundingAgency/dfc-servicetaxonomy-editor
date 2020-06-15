using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.HtmlBodyPartGraphSyncerTests
{
    public class HtmlBodyPartGraphSyncer_AddSyncComponentsTests
    {
        public dynamic? Content { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public HtmlBodyPartGraphSyncer HtmlBodyPartGraphSyncer { get; set; }

        public HtmlBodyPartGraphSyncer_AddSyncComponentsTests()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();
            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();
            GraphSyncHelper = A.Fake<IGraphSyncHelper>();

            HtmlBodyPartGraphSyncer = new HtmlBodyPartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_TitleInContent_TitleAddedToMergeNodeCommandsProperties()
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
        public async Task AddSyncComponents_NullTitleInContent_TitleNotAddedToMergeNodeCommandsProperties()
        {
            A.CallTo(() => GraphSyncHelper.PropertyName("Html")).Returns("htmlbody_Html");

            Content = JObject.Parse("{\"Html\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        private async Task CallAddSyncComponents()
        {
            await HtmlBodyPartGraphSyncer.AddSyncComponents(
                Content,
                MergeNodeCommand,
                ReplaceRelationshipsCommand,
                ContentTypePartDefinition,
                GraphSyncHelper);
        }
    }
}
