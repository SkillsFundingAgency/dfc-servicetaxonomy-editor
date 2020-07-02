using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.NumericFieldGraphSyncerTests
{
    //todo: factor out common code in tests
    public class NumericFieldGraphSyncer_AddSyncComponentsTests
    {
        public JObject? ContentItemField { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public IContentManager ContentManager { get; set; }
        public NumericFieldSettings NumericFieldSettings { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public NumericFieldGraphSyncer NumericFieldGraphSyncer { get; set; }

        const string _fieldName = "TestField";

        public NumericFieldGraphSyncer_AddSyncComponentsTests()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();

            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);

            ContentManager = A.Fake<IContentManager>();

            NumericFieldSettings = new NumericFieldSettings();
            A.CallTo(() => ContentPartFieldDefinition.GetSettings<NumericFieldSettings>()).Returns(NumericFieldSettings);

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);

            NumericFieldGraphSyncer = new NumericFieldGraphSyncer();
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
        //todo: assert that graphsynchelper's contenttype is not set

        private async Task CallAddSyncComponents()
        {
            await NumericFieldGraphSyncer.AddSyncComponents(
                ContentItemField!, TODO);
        }
    }
}
