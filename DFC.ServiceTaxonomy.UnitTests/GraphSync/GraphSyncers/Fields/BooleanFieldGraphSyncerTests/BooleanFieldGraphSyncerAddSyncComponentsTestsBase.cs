using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Fields;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.BooleanFieldGraphSyncerTests
{
    public class BooleanFieldGraphSyncerAddSyncComponentsTestsBase : FieldGraphSyncer_AddSyncComponentsTestsBase
    {
        public BooleanFieldGraphSyncerAddSyncComponentsTestsBase()
        {
            ContentFieldGraphSyncer = new BooleanFieldDataSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_BooleanInContent_BooleanAddedToMergeNodeCommandsProperties()
        {
            bool value = true;
            ContentItemField = JObject.Parse($"{{\"Value\": \"{value:o}\"}}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>
                {{_fieldName, value}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }
    }
}
