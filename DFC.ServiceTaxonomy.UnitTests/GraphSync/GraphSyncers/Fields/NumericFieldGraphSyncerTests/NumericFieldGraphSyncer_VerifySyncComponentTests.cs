using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.NumericFieldGraphSyncerTests
{
    public class NumericFieldGraphSyncer_VerifySyncComponentTests
    {
        public JObject? ContentItemField { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public NumericFieldSettings NumericFieldSettings { get; set; }
        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; set; }
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public IGraphValidationHelper GraphValidationHelper { get; set; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
        public NumericFieldGraphSyncer NumericFieldGraphSyncer { get; set; }

        const string _fieldName = "TestNumericFieldName";

        public NumericFieldGraphSyncer_VerifySyncComponentTests()
        {
            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);
            NumericFieldSettings = new NumericFieldSettings();
            A.CallTo(() => ContentPartFieldDefinition.GetSettings<NumericFieldSettings>()).Returns(NumericFieldSettings);

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);
            NodeWithOutgoingRelationships = A.Fake<INodeWithOutgoingRelationships>();
            A.CallTo(() => NodeWithOutgoingRelationships.SourceNode).Returns(SourceNode);

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);

            GraphValidationHelper = A.Fake<IGraphValidationHelper>();

            ExpectedRelationshipCounts = new Dictionary<string, int>();

            NumericFieldGraphSyncer = new NumericFieldGraphSyncer();
        }

        //todo: should we be strict when types mismatch, i.e. scale has changed? probably yes

        [Fact]
        public async Task VerifySyncComponent_Scale0PropertyCorrect_ReturnsTrue()
        {
            const string valueContent = "123.0";
            const long valueProperty = 123;

            ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");

            SourceNodeProperties.Add(_fieldName, valueProperty);

            NumericFieldSettings.Scale = 0;

            (bool verified, _) = await CallVerifySyncComponent();

            Assert.True(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_ScaleMoreThan0PropertyCorrect_ReturnsTrue()
        {
            const string valueContent = "123.0";
            const double valueProperty = 123d;

            ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");

            SourceNodeProperties.Add(_fieldName, valueProperty);

            NumericFieldSettings.Scale = 1;

            (bool verified, _) = await CallVerifySyncComponent();

            Assert.True(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_ContentNull_ReturnsFalse()
        {
            const long valueProperty = 123;

            ContentItemField = JObject.Parse($"{{\"Value\": null}}");

            SourceNodeProperties.Add(_fieldName, valueProperty);

            (bool verified, _) = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_PropertyMissing_ReturnsFalse()
        {
            const string valueContent = "123.0";
            ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");

            NumericFieldSettings.Scale = 0;

            (bool verified, _) = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_PropertySameScaleButValueDifferent_ReturnsFalse()
        {
            const string valueContent = "123.0";
            const long valueProperty = 321;

            ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");

            SourceNodeProperties.Add(_fieldName, valueProperty);

            NumericFieldSettings.Scale = 0;

            (bool verified, _) = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        // even though values are equivalent, types are different, so we fail validation
        [Fact]
        public async Task VerifySyncComponent_PropertyDecimalValueScale0ValueEquivalent_ReturnsFalse()
        {
            const string valueContent = "123.0";
            const double valueProperty = 123d;

            ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");

            SourceNodeProperties.Add(_fieldName, valueProperty);

            NumericFieldSettings.Scale = 0;

            (bool verified, _) = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_PropertyDecimalValueScale0PropertyValueMorePrecise_ReturnsFalse()
        {
            const string valueContent = "123.0";
            const double valueProperty = 123.4d;

            ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");

            SourceNodeProperties.Add(_fieldName, valueProperty);

            NumericFieldSettings.Scale = 0;

            (bool verified, _) = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_PropertyLongValueScaleMoreThan0ValueEquivalent_ReturnsFalse()
        {
            const string valueContent = "123.0";
            const long valueProperty = 123;

            ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");

            SourceNodeProperties.Add(_fieldName, valueProperty);

            NumericFieldSettings.Scale = 1;

            (bool verified, _) = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        //todo: test that verifies that failure reason is returned

        private async Task<(bool verified, string failureReason)> CallVerifySyncComponent()
        {
            return await NumericFieldGraphSyncer.VerifySyncComponent(
                ContentItemField!,
                ContentPartFieldDefinition,
                NodeWithOutgoingRelationships,
                GraphSyncHelper,
                GraphValidationHelper,
                ExpectedRelationshipCounts);
        }
    }
}
