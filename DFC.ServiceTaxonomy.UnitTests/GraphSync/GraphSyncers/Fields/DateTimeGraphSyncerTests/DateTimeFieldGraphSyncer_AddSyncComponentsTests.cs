using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.FieldGraphSyncer;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.DateTimeFieldGraphSyncerTests
{
    //todo: factor out common code in tests
    public class DateTimeFieldGraphSyncer_AddSyncComponentsTests : FieldGraphSyncer_AddSyncComponentsTests
    {
        public DateTimeFieldGraphSyncer_AddSyncComponentsTests()
        {
            ContentFieldGraphSyncer = new DateTimeFieldGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_DateTimeInContent_DateTimeAddedToMergeNodeCommandsProperties()
        {
            DateTime value = new DateTime(2019, 2, 18, 20, 14, 13);
            ContentItemField = JObject.Parse($"{{\"Value\": \"{value:o}\"}}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>
                {{_fieldName, value}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullDateTimeInContent_DateTimeNotAddedToMergeNodeCommandsProperties()
        {
            ContentItemField = JObject.Parse("{\"Value\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        //todo: assert that nothing else is done to the commands
        //todo: assert that syncnameprovider's contenttype is not set
    }
}
