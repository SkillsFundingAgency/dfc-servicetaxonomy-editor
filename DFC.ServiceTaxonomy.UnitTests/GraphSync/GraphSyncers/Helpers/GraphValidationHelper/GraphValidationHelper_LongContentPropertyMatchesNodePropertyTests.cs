using System.Collections.Generic;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Helpers.GraphValidationHelper
{
    public class GraphValidationHelper_LongContentPropertyMatchesNodePropertyTests
    {
        public const string ContentKey = "Long";
        public JObject ContentItemField { get; set; }
        public const string NodePropertyName = "nodePropertyName";
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper GraphValidationHelper { get; set; }

        public GraphValidationHelper_LongContentPropertyMatchesNodePropertyTests()
        {
            ContentItemField = JObject.Parse("{}");

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);

            GraphValidationHelper = new ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper();
        }

        [Theory]
        [InlineData(long.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(long.MaxValue)]
        public void LongContentPropertyMatchesNodeProperty_PropertyCorrect_ReturnsTrue(long value)
        {
            string json = $"{{\"{ContentKey}\": {value}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, value);

            (bool validated, _) = CallLongContentPropertyMatchesNodeProperty();

            Assert.True(validated);
        }

        [Theory]
        [InlineData(long.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(long.MaxValue)]
        public void LongContentPropertyMatchesNodeProperty_PropertyMissing_ReturnsFalse(long value)
        {
            string json = $"{{\"{ContentKey}\": {value}}}";
            ContentItemField = JObject.Parse(json);

            (bool validated, _) = CallLongContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Theory]
        [InlineData(long.MinValue, 0)]
        [InlineData(-1, long.MinValue)]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(long.MaxValue, 0)]
        public void LongContentPropertyMatchesNodeProperty_PropertiesSameTypeButDifferentValues_ReturnsFalse(long contentValue, long nodeValue)
        {
            string json = $"{{\"{ContentKey}\": {contentValue}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (bool validated, _) = CallLongContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Theory]
        [InlineData(-1, "string")]
        [InlineData(0, "string")]
        [InlineData(1, "string")]
        [InlineData(-1, true)]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(1, false)]
        //todo: other valid neo property types
        public void LongContentPropertyMatchesNodeProperty_PropertiesDifferentTypes_ReturnsFalse(long contentValue, object nodeValue)
        {
            string json = $"{{\"{ContentKey}\": {contentValue}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (bool validated, _) = CallLongContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Theory]
        [InlineData("content property value was '-1', but node property value was '1'", -1, 1)]
        [InlineData("content property value was '1', but node property value was '-1'", 1, -1)]
        [InlineData("content property value was '0', but node property value was '1'", 0, 1)]
        [InlineData("content property value was '0', but node property value was '-1'", 0, -1)]
        public void LongContentPropertyMatchesNodeProperty_PropertySameTypeButDifferent_ReturnsFailedValidationMessage(string expectedMessage, long contentValue, long nodeValue)
        {
            string json = $"{{\"{ContentKey}\": {contentValue}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (_, string message) = CallLongContentPropertyMatchesNodeProperty();

            Assert.Equal(expectedMessage, message);
        }

        //todo: test message when property types different

        private (bool matched, string failureReason) CallLongContentPropertyMatchesNodeProperty()
        {
            return GraphValidationHelper.LongContentPropertyMatchesNodeProperty(
                ContentKey,
                ContentItemField,
                NodePropertyName,
                SourceNode);
        }
    }
}
