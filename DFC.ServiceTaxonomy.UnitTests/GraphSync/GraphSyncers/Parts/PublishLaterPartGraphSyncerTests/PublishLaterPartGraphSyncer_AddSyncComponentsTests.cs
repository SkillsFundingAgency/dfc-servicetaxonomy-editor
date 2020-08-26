using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.PartGraphSyncer;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.HtmlBodyPartGraphSyncerTests
{
    public class PublishLaterPartGraphSyncer_AddSyncComponentsTests : PartGraphSyncer_AddSyncComponentsTests
    {
        public PublishLaterPartGraphSyncer_AddSyncComponentsTests()
        {
            ContentPartGraphSyncer = new PublishLaterPartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_ScheduledPublishUtcContent_TitleAddedToMergeNodeCommandsProperties()
        {
            A.CallTo(() => SyncNameProvider.PropertyName("ScheduledPublishUtc")).Returns("publishlater_ScheduledPublishUtc");

            const string scheduledDateUtc = "2020-06-28T09:58:00Z";
            DateTime expectedDateUtc = new DateTime(2020, 6, 28, 9, 58, 0, DateTimeKind.Utc);

            Content = JObject.Parse($"{{\"ScheduledPublishUtc\": \"{scheduledDateUtc}\"}}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>
                {{"publishlater_ScheduledPublishUtc", expectedDateUtc}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullScheduledPublicUtcInContent_TitleNotAddedToMergeNodeCommandsProperties()
        {
            A.CallTo(() => SyncNameProvider.PropertyName("ScheduledPublishUtc")).Returns("publishlater_ScheduledPublishUtc");

            Content = JObject.Parse("{\"ScheduledPublishUtc\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }
    }
}
