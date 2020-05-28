using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.DateTimeFieldGraphSyncerTests
{
    //todo: factor out common code in tests
    public class DateTimeFieldGraphSyncer_AddSyncComponentsTests
    {
        public JObject? ContentItemField { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public DateTimeFieldGraphSyncer DateTimeFieldGraphSyncer { get; set; }

        const string _fieldName = "TestField";

        public DateTimeFieldGraphSyncer_AddSyncComponentsTests()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();

            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);

            DateTimeFieldGraphSyncer = new DateTimeFieldGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_DateTimeInContent_DateTimeAddedToMergeNodeCommandsProperties()
        {
            DateTime value = new DateTime(2019, 2, 18, 20, 14, 13);
            ContentItemField = JObject.Parse($"{{\"Value\": \"{value.ToString("o")}\"}}");

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
        //todo: assert that graphsynchelper's contenttype is not set

        private async Task CallAddSyncComponents()
        {
            await DateTimeFieldGraphSyncer.AddSyncComponents(
                ContentItemField!,
                MergeNodeCommand,
                ReplaceRelationshipsCommand,
                ContentPartFieldDefinition,
                GraphSyncHelper);
        }
    }
}
