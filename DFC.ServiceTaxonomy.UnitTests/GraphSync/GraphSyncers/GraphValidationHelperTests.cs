using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers
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
        public void GraphValidationHelper_PropertyCorrect_ReturnsTrue()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            SourceNodeProperties.Add(NodePropertyName, text);

            (bool verified, _) = CallStringContentPropertyMatchesNodeProperty();

            Assert.True(verified);
        }

        [Fact]
        public void GraphValidationHelper_PropertyMissing_ReturnsFalse()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            (bool verified, _) = CallStringContentPropertyMatchesNodeProperty();

            Assert.False(verified);
        }

        [Fact]
        public void VerifySyncComponent_PropertyDifferent_ReturnsFalse()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            SourceNodeProperties.Add(NodePropertyName, "some_other_value");

            (bool verified, _) = CallStringContentPropertyMatchesNodeProperty();

            Assert.False(verified);
        }

        //todo: tests for failureMessage

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
