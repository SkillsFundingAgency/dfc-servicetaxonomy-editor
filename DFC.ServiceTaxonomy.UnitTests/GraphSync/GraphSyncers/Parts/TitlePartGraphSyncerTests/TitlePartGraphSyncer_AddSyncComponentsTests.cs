using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.TitlePartGraphSyncerTests
{
    public class TitlePartGraphSyncer_AddSyncComponentsTests
    {
        public JObject Content { get; set; }
        public ContentItem ContentItem { get; set; }
        public IContentManager ContentManager { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public TitlePartGraphSyncer TitlePartGraphSyncer { get; set; }

        public const string NodeTitlePropertyName = "skos__prefLabel";

        public TitlePartGraphSyncer_AddSyncComponentsTests()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();
            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();
            GraphSyncHelper = A.Fake<IGraphSyncHelper>();

            Content = JObject.Parse("{}");
            ContentItem = A.Fake<ContentItem>();
            ContentManager = A.Fake<IContentManager>();

            TitlePartGraphSyncer = new TitlePartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_TitleInContent_TitleAddedToMergeNodeCommandsProperties()
        {
            const string title = "title";

            Content = JObject.Parse($"{{\"Title\": \"{title}\"}}");

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
                {{NodeTitlePropertyName, title}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullTitleInContent_DisplayTextAddedToMergeNodeCommandsProperties()
        {
            const string displayText = "DisplayText";
            Content = JObject.Parse("{\"Title\": null}");
            ContentItem.DisplayText = displayText;

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>
                {{NodeTitlePropertyName, displayText}};
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        private async Task CallAddSyncComponents()
        {
            await TitlePartGraphSyncer.AddSyncComponents(
                Content,
                ContentItem,
                ContentManager,
                MergeNodeCommand,
                ReplaceRelationshipsCommand,
                ContentTypePartDefinition,
                GraphSyncHelper);
        }
    }
}
