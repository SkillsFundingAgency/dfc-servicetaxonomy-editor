using System;
using System.Collections.Generic;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Helpers.GraphValidationHelper
{
    public class GraphValidationHelper_EnumContentPropertyMatchesNodePropertyTests
    {
        enum TestEnum1
        {
            Alice,
            Bob,
            Charlie
        }

        enum TestEnum2
        {
            Spam,
            Ham,
            Eggs
        }

        public const string ContentKey = "Enum";
        public JObject ContentItemField { get; set; }
        public const string NodePropertyName = "nodePropertyName";
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper GraphValidationHelper { get; set; }

        public GraphValidationHelper_EnumContentPropertyMatchesNodePropertyTests()
        {
            ContentItemField = JObject.Parse("{}");

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);

            GraphValidationHelper = new ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper();
        }

        [Theory]
        [InlineData(TestEnum1.Bob)]
        [InlineData(TestEnum1.Charlie)]
        [InlineData(TestEnum2.Spam)]
        public void EnumContentPropertyMatchesNodeProperty_PropertyCorrect_ReturnsTrue<T>(T value)
            where T : Enum
        {
            string json = $"{{\"{ContentKey}\": {(int)(object)value}}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, value);

            (bool validated, _) = CallEnumContentPropertyMatchesNodeProperty<T>();

            Assert.True(validated);
        }

        [Theory]
        [InlineData(TestEnum1.Alice)]
        [InlineData(TestEnum2.Ham)]
        [InlineData(TestEnum2.Eggs)]
        public void EnumContentPropertyMatchesNodeProperty_PropertyMissing_ReturnsFalse<T>(T value)
            where T : Enum
        {
            string json = $"{{\"{ContentKey}\": \"{(int)(object)value}\"}}";
            ContentItemField = JObject.Parse(json);

            (bool validated, _) = CallEnumContentPropertyMatchesNodeProperty<T>();

            Assert.False(validated);
        }

        [Theory]
        [InlineData(TestEnum1.Alice, TestEnum1.Bob)]
        [InlineData(TestEnum2.Eggs, TestEnum2.Spam)]
        public void EnumContentPropertyMatchesNodeProperty_PropertiesSameTypeButDifferentValues_ReturnsFalse<T>(T contentValue, T nodeValue)
            where T : Enum
        {
            string json = $"{{\"{ContentKey}\": \"{(int)(object)contentValue}\"}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (bool validated, _) = CallEnumContentPropertyMatchesNodeProperty<T>();

            Assert.False(validated);
        }

        [Theory]
        [InlineData(TestEnum1.Charlie, "node value is string, different string")]
        [InlineData(TestEnum1.Alice, 0)]
        [InlineData(TestEnum1.Bob, 1)]
        [InlineData(TestEnum2.Spam, true)]
        [InlineData(TestEnum2.Spam, false)]
        [InlineData(TestEnum2.Ham, true)]
        [InlineData(TestEnum2.Ham, false)]
        [InlineData(TestEnum1.Alice, TestEnum2.Spam)]
        //todo: other valid neo property types
        public void EnumContentPropertyMatchesNodeProperty_PropertiesDifferentTypes_ReturnsFalse<T>(T contentValue, object nodeValue)
            where T : Enum
        {
            string json = $"{{\"{ContentKey}\": \"{(int)(object)contentValue}\"}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (bool validated, _) = CallEnumContentPropertyMatchesNodeProperty<T>();

            Assert.False(validated);
        }

        [Theory]
        [InlineData("content property value was '0', but node property value was 'Bob'", TestEnum1.Alice, TestEnum1.Bob)]
        [InlineData("content property value was '0', but node property value was 'Charlie'", TestEnum1.Alice, TestEnum1.Charlie)]
        public void EnumContentPropertyMatchesNodeProperty_PropertySameTypeButDifferent_ReturnsFailedValidationMessage<T>(string expectedMessage, T contentValue, T nodeValue)
            where T : Enum
        {
            string json = $"{{\"{ContentKey}\": \"{(int)(object)contentValue}\"}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (_, string message) = CallEnumContentPropertyMatchesNodeProperty<T>();

            Assert.Equal(expectedMessage, message);
        }

        //todo: test message when property types different

        private (bool matched, string failureReason) CallEnumContentPropertyMatchesNodeProperty<T>()
            where T : Enum
        {
            return GraphValidationHelper.EnumContentPropertyMatchesNodeProperty<T>(
                ContentKey,
                ContentItemField,
                NodePropertyName,
                SourceNode);
        }
    }
}
