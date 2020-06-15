using System.Collections.Generic;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Helpers.GraphValidationHelper
{
    public class GraphValidationHelperBoolTests
    {
        public const string ContentKey = "Bool";
        public JObject ContentItemField { get; set; }
        public const string NodePropertyName = "nodePropertyName";
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper GraphValidationHelper { get; set; }

        public GraphValidationHelperBoolTests()
        {
            ContentItemField = JObject.Parse("{}");

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);

            GraphValidationHelper = new ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BoolContentPropertyMatchesNodeProperty_PropertyCorrect_ReturnsTrue(bool value)
        {
            string json = $"{{\"{ContentKey}\": {value.ToString().ToLower()}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, value);

            (bool validated, _) = CallBoolContentPropertyMatchesNodeProperty();

            Assert.True(validated);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BoolContentPropertyMatchesNodeProperty_PropertyMissing_ReturnsFalse(bool value)
        {
            string json = $"{{\"{ContentKey}\": {value.ToString().ToLower()}}}";
            ContentItemField = JObject.Parse(json);

            (bool validated, _) = CallBoolContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void BoolContentPropertyMatchesNodeProperty_PropertySameTypeButDifferent_ReturnsFalse(bool contentValue, bool nodeValue)
        {
            string json = $"{{\"{ContentKey}\": {contentValue.ToString().ToLower()}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (bool validated, _) = CallBoolContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Theory]
        [InlineData("content property value was 'True', but node property value was 'False'", true, false)]
        [InlineData("content property value was 'False', but node property value was 'True'", false, true)]
        public void BoolContentPropertyMatchesNodeProperty_PropertySameTypeButDifferent_ReturnsFailedValidationMessage(string expectedMessage, bool contentValue, bool nodeValue)
        {
            string json = $"{{\"{ContentKey}\": {contentValue.ToString().ToLower()}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (_, string message) = CallBoolContentPropertyMatchesNodeProperty();

            Assert.Equal(expectedMessage, message);
        }

        //todo: different types tests

        private (bool matched, string failureReason) CallBoolContentPropertyMatchesNodeProperty()
        {
            return GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                ContentKey,
                ContentItemField,
                NodePropertyName,
                SourceNode);
        }
    }
}
