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

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.SitemapPartGraphSyncerTests
{
    public class SitemapPartGraphSyncer_AddSyncComponentsTests
    {
        public JObject Content { get; set; }
        public ContentItem ContentItem { get; set; }
        public IContentManager ContentManager { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public SitemapPartGraphSyncer SitemapPartGraphSyncer { get; set; }

        public SitemapPartGraphSyncer_AddSyncComponentsTests()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();
            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();
            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName("OverrideSitemapConfig")).Returns("sitemap_OverrideSitemapConfig");
            A.CallTo(() => GraphSyncHelper.PropertyName("ChangeFrequency")).Returns("sitemap_ChangeFrequency");
            A.CallTo(() => GraphSyncHelper.PropertyName("Priority")).Returns("sitemap_Priority");
            A.CallTo(() => GraphSyncHelper.PropertyName("Exclude")).Returns("sitemap_Exclude");

            Content = JObject.Parse("{}");
            ContentItem = A.Fake<ContentItem>();
            ContentManager = A.Fake<IContentManager>();

            SitemapPartGraphSyncer = new SitemapPartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_AllPropertiesInContent_AllPropertiesAddedToMergeNodeCommandsProperties()
        {
            Content = JObject.Parse(@$"{{""OverrideSitemapConfig"": ""{true.ToString().ToLower()}"",
                                        ""ChangeFrequency"": 2, ""Priority"": 1, ""Exclude"": false}}");

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
            {
                {"sitemap_OverrideSitemapConfig", true},
                {"sitemap_ChangeFrequency", "Weekly"},
                {"sitemap_Priority", 1},
                {"sitemap_Exclude", false}
            };

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        //todo: split out tests

        private async Task CallAddSyncComponents()
        {
            await SitemapPartGraphSyncer.AddSyncComponents(
                Content, TODO);
        }
    }
}
