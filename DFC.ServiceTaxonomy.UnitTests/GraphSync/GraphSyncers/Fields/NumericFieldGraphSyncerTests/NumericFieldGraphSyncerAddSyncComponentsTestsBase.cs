using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.NumericFieldGraphSyncerTests
{
    public class NumericFieldGraphSyncerAddSyncComponentsTestsBase : FieldGraphSyncer_AddSyncComponentsTestsBase
    {
        public NumericFieldSettings NumericFieldSettings { get; set; }

        public NumericFieldGraphSyncerAddSyncComponentsTestsBase()
        {
            NumericFieldSettings = new NumericFieldSettings();
            A.CallTo(() => ContentPartFieldDefinition.GetSettings<NumericFieldSettings>()).Returns(NumericFieldSettings);

            ContentFieldGraphSyncer = new NumericFieldGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_Scale0NumericInContent_IntAddedToMergeNodeCommandsProperties()
        {
            const string value = "123.0";
            const int expectedValue = 123;

            //todo: JValue's type is being set to Object. code under test doesn't rely on it being Float like within oc, but should arrange as close to oc as possible
            //todo: make sure type is set correctly in all unit tests
            ContentItemField = JObject.Parse($"{{\"Value\": {value}}}");

            NumericFieldSettings.Scale = 0;

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
                {{_fieldName, expectedValue}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_Scale1NumericInContent_DecimalAddedToMergeNodeCommandsProperties()
        {
            const string value = "123.4";
            const decimal expectedValue = 123.4m;

            ContentItemField = JObject.Parse($"{{\"Value\": {value}}}");

            NumericFieldSettings.Scale = 1;

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
                {{_fieldName, expectedValue}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullNumericInContent_NumericNotAddedToMergeNodeCommandsProperties()
        {
            ContentItemField = JObject.Parse("{\"Value\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        //todo: assert that nothing else is done to the commands
        //todo: assert that SyncNameProvider's contenttype is not set
    }
}
