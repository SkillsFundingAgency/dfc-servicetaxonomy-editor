using System;
using System.Collections.Generic;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Helpers.GraphValidationHelper
{
    public class GraphValidationHelper_ContentPropertyMatchesNodePropertyTests
    {
        public const string ContentKey = "Text";
        public JObject ContentItemField { get; set; }
        public const string NodePropertyName = "nodePropertyName";
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public Func<JValue, object, bool> AreBothSame { get; set; }
        public ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper GraphValidationHelper { get; set; }

        public GraphValidationHelper_ContentPropertyMatchesNodePropertyTests()
        {
            ContentItemField = JObject.Parse("{}");

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);

            GraphValidationHelper = new ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper();

            AreBothSame = (value, o) => true;
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void ContentPropertyMatchesNodeProperty_ContentAndNodeHaveAValue_ReturnsCallbackReturn(bool expectedValidated, bool callbackReturn)
        {
            ContentItemField = JObject.Parse($"{{\"{ContentKey}\": \"ContentValue\"}}");

            SourceNodeProperties.Add(NodePropertyName, "NodeValue");

            AreBothSame = (value, o) => callbackReturn;

            (bool validated, _) = CallContentPropertyMatchesNodeProperty();

            Assert.Equal(expectedValidated, validated);
        }

        private (bool matched, string failureReason) CallContentPropertyMatchesNodeProperty()
        {
            return GraphValidationHelper.ContentPropertyMatchesNodeProperty(
                ContentKey,
                ContentItemField,
                NodePropertyName,
                SourceNode,
                AreBothSame);
        }
    }
}
