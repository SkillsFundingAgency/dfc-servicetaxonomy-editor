using System.Collections.Generic;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Helpers.GraphValidationHelper
{
    public class GraphValidationHelperIntTests
    {
        public const string ContentKey = "Int";
        public JObject ContentItemField { get; set; }
        public const string NodePropertyName = "nodePropertyName";
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper GraphValidationHelper { get; set; }

        public GraphValidationHelperIntTests()
        {
            ContentItemField = JObject.Parse("{}");

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);

            GraphValidationHelper = new ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper();
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void IntContentPropertyMatchesNodeProperty_PropertyCorrect_ReturnsTrue(int value)
        {
            string json = $"{{\"{ContentKey}\": {value}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, value);

            (bool validated, _) = CallIntContentPropertyMatchesNodeProperty();

            Assert.True(validated);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void IntContentPropertyMatchesNodeProperty_PropertyMissing_ReturnsFalse(int value)
        {
            string json = $"{{\"{ContentKey}\": {value}}}";
            ContentItemField = JObject.Parse(json);

            (bool validated, _) = CallIntContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Theory]
        [InlineData(int.MinValue, 0)]
        [InlineData(-1, int.MinValue)]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(int.MaxValue, 0)]
        public void IntContentPropertyMatchesNodeProperty_PropertiesSameTypeButDifferentValues_ReturnsFalse(int contentValue, int nodeValue)
        {
            string json = $"{{\"{ContentKey}\": {contentValue}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (bool validated, _) = CallIntContentPropertyMatchesNodeProperty();

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
        public void IntContentPropertyMatchesNodeProperty_PropertiesDifferentTypes_ReturnsFalse(int contentValue, object nodeValue)
        {
            string json = $"{{\"{ContentKey}\": {contentValue}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (bool validated, _) = CallIntContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Theory]
        [InlineData("content property value was '-1', but node property value was '1'", -1, 1)]
        [InlineData("content property value was '1', but node property value was '-1'", 1, -1)]
        [InlineData("content property value was '0', but node property value was '1'", 0, 1)]
        [InlineData("content property value was '0', but node property value was '-1'", 0, -1)]
        public void IntContentPropertyMatchesNodeProperty_PropertySameTypeButDifferent_ReturnsFailedValidationMessage(string expectedMessage, int contentValue, int nodeValue)
        {
            string json = $"{{\"{ContentKey}\": {contentValue}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (_, string message) = CallIntContentPropertyMatchesNodeProperty();

            Assert.Equal(expectedMessage, message);
        }

        //todo: test message when property types different

        private (bool matched, string failureReason) CallIntContentPropertyMatchesNodeProperty()
        {
            return GraphValidationHelper.IntContentPropertyMatchesNodeProperty(
                ContentKey,
                ContentItemField,
                NodePropertyName,
                SourceNode);
        }
    }
}
