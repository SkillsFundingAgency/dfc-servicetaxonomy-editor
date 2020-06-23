using System;
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

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.HtmlBodyPartGraphSyncerTests
{
    public class PublishLaterPartGraphSyncer_AddSyncComponentsTests
    {
        public JObject Content { get; set; }
        public ContentItem ContentItem { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public PublishLaterPartGraphSyncer PublishLaterPartGraphSyncer { get; set; }

        public PublishLaterPartGraphSyncer_AddSyncComponentsTests()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();
            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();
            GraphSyncHelper = A.Fake<IGraphSyncHelper>();

            Content = JObject.Parse("{}");
            ContentItem = A.Fake<ContentItem>();

            PublishLaterPartGraphSyncer = new PublishLaterPartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_ScheduledPublishUtcContent_TitleAddedToMergeNodeCommandsProperties()
        {
            A.CallTo(() => GraphSyncHelper.PropertyName("ScheduledPublishUtc")).Returns("publishlater_ScheduledPublishUtc");

            const string scheduledDateUtc = "2020-06-28T09:58:00Z";

            Content = JObject.Parse($"{{\"ScheduledPublishUtc\": \"{scheduledDateUtc}\"}}");

            await CallAddSyncComponents();

            //Date compare culture is skewed and ends up an hour different
            IDictionary<string, object> expectedProperties = new Dictionary<string, object>
                {{"publishlater_ScheduledPublishUtc", DateTime.Parse(scheduledDateUtc).AddHours(-1)}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullScheduledPublicUtcInContent_TitleNotAddedToMergeNodeCommandsProperties()
        {
            A.CallTo(() => GraphSyncHelper.PropertyName("ScheduledPublishUtc")).Returns("publishlater_ScheduledPublishUtc");

            Content = JObject.Parse("{\"ScheduledPublishUtc\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        private async Task CallAddSyncComponents()
        {
            await PublishLaterPartGraphSyncer.AddSyncComponents(
                Content,
                ContentItem,
                MergeNodeCommand,
                ReplaceRelationshipsCommand,
                ContentTypePartDefinition,
                GraphSyncHelper);
        }
    }
}
