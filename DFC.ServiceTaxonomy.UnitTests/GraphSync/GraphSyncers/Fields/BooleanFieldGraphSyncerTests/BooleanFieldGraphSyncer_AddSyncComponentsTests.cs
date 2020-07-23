using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.FieldGraphSyncer;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.BooleanFieldGraphSyncerTests
{
    public class BooleanFieldGraphSyncer_AddSyncComponentsTests : FieldGraphSyncer_AddSyncComponentsTests
    {
        public BooleanFieldGraphSyncer_AddSyncComponentsTests()
        {
            ContentFieldGraphSyncer = new BooleanFieldGraphSyncer();
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
