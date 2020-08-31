using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.UnpublishLaterPartGraphSyncerTests
{
    public class UnpublishLaterPartGraphSyncerAddSyncComponentsTestsBase : PartGraphSyncer_AddSyncComponentsTestsBase
    {
        public const string NodeTitlePropertyName = "unpublishlater_ScheduledUnpublishUtc";

        public UnpublishLaterPartGraphSyncerAddSyncComponentsTestsBase()
        {
            ContentPartGraphSyncer = new UnpublishLaterPartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_ScheduledUnpublishUtcContent_TitleAddedToMergeNodeCommandsProperties()
        {
            const string scheduledDateUtc = "2020-06-28T09:58:00Z";
            DateTime expectedDateUtc = new DateTime(2020, 6, 28, 9, 58, 0, DateTimeKind.Utc);

            Content = JObject.Parse($"{{\"ScheduledUnpublishUtc\": \"{scheduledDateUtc}\"}}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>
                {{NodeTitlePropertyName, expectedDateUtc}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullScheduledPublicUtcInContent_TitleNotAddedToMergeNodeCommandsProperties()
        {
            Content = JObject.Parse("{\"ScheduledUnpublishUtc\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }
    }
}
