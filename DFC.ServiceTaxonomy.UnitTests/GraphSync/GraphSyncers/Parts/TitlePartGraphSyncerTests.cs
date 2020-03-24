using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts
{
    public class TitlePartGraphSyncerTests
    {
        public dynamic? Content { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public TitlePartGraphSyncer TitlePartGraphSyncer { get; set; }

        public TitlePartGraphSyncerTests()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();
            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();
            GraphSyncHelper = A.Fake<IGraphSyncHelper>();

            TitlePartGraphSyncer = new TitlePartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_TitleInContent_TitleAddedToMergeNodeCommandsProperties()
        {
            const string title = "title";

            Content = JObject.Parse($"{{\"Title\": \"{title}\"}}");

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
                {{"skos__prefLabel", title}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        // not approp for title
        // [Fact]
        // public async Task AddSyncComponents_NoTitleInContent_TitleNotAddedToMergeNodeCommandsProperties()
        // {
        //     Content = JObject.Parse("{}");
        //
        //     await CallAddSyncComponents();
        //
        //     IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
        //     Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        // }

        //check empty behaviou
        // [Fact]
        // public async Task AddSyncComponents_NullTitleInContent_TitleNotAddedToMergeNodeCommandsProperties()
        // {
        //     Content = JObject.Parse("{\"Title\": null}");
        //
        //     await CallAddSyncComponents();
        //
        //     IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
        //     Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        // }

        private async Task CallAddSyncComponents()
        {
            await TitlePartGraphSyncer.AddSyncComponents(
                Content,
                MergeNodeCommand,
                ReplaceRelationshipsCommand,
                ContentTypePartDefinition,
                GraphSyncHelper);
        }
    }
}
