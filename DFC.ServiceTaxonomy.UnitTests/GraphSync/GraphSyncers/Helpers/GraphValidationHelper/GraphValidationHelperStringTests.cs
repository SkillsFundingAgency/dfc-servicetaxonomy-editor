using System.Collections.Generic;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Helpers.GraphValidationHelper
{
    public class GraphValidationHelperStringTests
    {
        public const string ContentKey = "Text";
        public JObject ContentItemField { get; set; }
        public const string NodePropertyName = "nodePropertyName";
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper GraphValidationHelper { get; set; }

        public GraphValidationHelperStringTests()
        {
            ContentItemField = JObject.Parse("{}");

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);

            GraphValidationHelper = new ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper();
        }

        [Fact]
        public void StringContentPropertyMatchesNodeProperty_PropertyCorrect_ReturnsTrue()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"{ContentKey}\": \"{text}\"}}");

            SourceNodeProperties.Add(NodePropertyName, text);

            (bool validated, _) = CallStringContentPropertyMatchesNodeProperty();

            Assert.True(validated);
        }

        [Fact]
        public void StringContentPropertyMatchesNodeProperty_PropertyMissing_ReturnsFalse()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"{ContentKey}\": \"{text}\"}}");

            (bool validated, _) = CallStringContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Fact]
        public void StringContentPropertyMatchesNodeProperty_PropertyDifferent_ReturnsFalse()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"{ContentKey}\": \"{text}\"}}");

            SourceNodeProperties.Add(NodePropertyName, "some_other_value");

            (bool validated, _) = CallStringContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Theory]
        [InlineData("content property value was 'contentValue', but node property value was 'nodeValue'", "contentValue", "nodeValue")]
        [InlineData("content property value was 'contentValue', but node property value was ''", "contentValue", "")]
        public void StringContentPropertyMatchesNodeProperty_PropertySameTypeButDifferent_ReturnsFailedValidationMessage(string expectedMessage, string contentValue, string nodeValue)
        {
            string json = $"{{\"{ContentKey}\": \"{contentValue}\"}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (_, string message) = CallStringContentPropertyMatchesNodeProperty();

            Assert.Equal(expectedMessage, message);
        }

        //todo: tests for ValidateOutgoingRelationship

        private (bool matched, string failureReason) CallStringContentPropertyMatchesNodeProperty()
        {
            return GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKey,
                ContentItemField,
                NodePropertyName,
                SourceNode);
        }
    }
}
