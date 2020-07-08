using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.PartGraphSyncer;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.UnpublishLaterPartGraphSyncerTests
{
    public class UnpublishLaterPartGraphSyncer_AddSyncComponentsTests : PartGraphSyncer_AddSyncComponentsTests
    {
        public const string NodeTitlePropertyName = "unpublishlater_ScheduledUnpublishUtc";

        public UnpublishLaterPartGraphSyncer_AddSyncComponentsTests()
        {
            ContentPartGraphSyncer = new UnpublishLaterPartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_ScheduledUnpublishUtcContent_TitleAddedToMergeNodeCommandsProperties()
        {
            const string scheduledDateUtc = "2020-06-28T09:58:00Z";

            Content = JObject.Parse($"{{\"ScheduledUnpublishUtc\": \"{scheduledDateUtc}\"}}");

            await CallAddSyncComponents();

            //Date compare culture is skewed and ends up an hour different
            IDictionary<string, object> expectedProperties = new Dictionary<string, object>
                {{NodeTitlePropertyName, DateTime.Parse(scheduledDateUtc).AddHours(-1)}};

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
