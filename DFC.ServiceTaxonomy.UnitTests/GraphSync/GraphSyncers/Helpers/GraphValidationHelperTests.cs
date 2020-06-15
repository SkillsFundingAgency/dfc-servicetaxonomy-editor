using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Helpers
{
    public class GraphValidationHelperTests
    {
        public string ContentKey = "Text";
        public JObject ContentItemField { get; set; }
        public string NodePropertyName = "nodePropertyName";
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public GraphValidationHelper GraphValidationHelper { get; set; }

        public GraphValidationHelperTests()
        {
            ContentItemField = JObject.Parse("{}");

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);

            GraphValidationHelper = new GraphValidationHelper();
        }

        [Fact]
        public void StringContentPropertyMatchesNodeProperty_PropertyCorrect_ReturnsTrue()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            SourceNodeProperties.Add(NodePropertyName, text);

            (bool validated, _) = CallStringContentPropertyMatchesNodeProperty();

            Assert.True(validated);
        }

        [Fact]
        public void StringContentPropertyMatchesNodeProperty_PropertyMissing_ReturnsFalse()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            (bool validated, _) = CallStringContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Fact]
        public void StringContentPropertyMatchesNodeProperty_PropertyDifferent_ReturnsFalse()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            SourceNodeProperties.Add(NodePropertyName, "some_other_value");

            (bool validated, _) = CallStringContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        //todo: tests for failureMessage
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
