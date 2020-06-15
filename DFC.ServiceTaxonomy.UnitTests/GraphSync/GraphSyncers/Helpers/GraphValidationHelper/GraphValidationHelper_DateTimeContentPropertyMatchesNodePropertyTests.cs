using System;
using System.Collections.Generic;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Helpers.GraphValidationHelper
{
    public class GraphValidationHelper_DateTimeContentPropertyMatchesNodePropertyTests
    {
        public const string ContentKey = "DateTime";
        public JObject ContentItemField { get; set; }
        public const string NodePropertyName = "nodePropertyName";
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper GraphValidationHelper { get; set; }

        public GraphValidationHelper_DateTimeContentPropertyMatchesNodePropertyTests()
        {
            ContentItemField = JObject.Parse("{}");

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);

            GraphValidationHelper = new ServiceTaxonomy.GraphSync.GraphSyncers.Helpers.GraphValidationHelper();
        }

        [Fact]
        public void DateTimeContentPropertyMatchesNodeProperty_PropertyCorrect_ReturnsTrue()
        {
            const string contentDateTimeString = "2020-06-15T14:24:00Z";
            DateTime dateTime = DateTime.Parse(contentDateTimeString);
            ZonedDateTime nodeZonedDateTime = new ZonedDateTime(dateTime.ToUniversalTime());

            ContentItemField = JObject.Parse($"{{\"{ContentKey}\": \"{contentDateTimeString}\"}}");

            SourceNodeProperties.Add(NodePropertyName, nodeZonedDateTime);

            (bool validated, _) = CallDateTimeContentPropertyMatchesNodeProperty();

            Assert.True(validated);
        }

        [Fact]
        public void DateTimeContentPropertyMatchesNodeProperty_PropertyMissing_ReturnsFalse()
        {
            const string contentDateTimeString = "2020-06-15T14:24:00Z";
            ContentItemField = JObject.Parse($"{{\"{ContentKey}\": \"{contentDateTimeString}\"}}");

            (bool validated, _) = CallDateTimeContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Fact]
        public void DateTimeContentPropertyMatchesNodeProperty_PropertiesSameTypeButDifferentValues_ReturnsFalse()
        {
            const string contentDateTimeString = "2020-06-15T14:24:00Z";
            ZonedDateTime nodeZonedDateTime = new ZonedDateTime(2021, 1, 1, 16, 0, 0, Zone.Of(0));

            ContentItemField = JObject.Parse($"{{\"{ContentKey}\": \"{contentDateTimeString}\"}}");

            SourceNodeProperties.Add(NodePropertyName, nodeZonedDateTime);

            (bool validated, _) = CallDateTimeContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData("")]
        [InlineData(0)]
        [InlineData("2020-06-15T14:24:00Z")]
        //todo: other valid neo property types
        public void DateTimeContentPropertyMatchesNodeProperty_PropertiesDifferentTypes_ReturnsFalse(object nodeValue)
        {
            const string contentDateTimeString = "2020-06-15T14:24:00Z";

            string json = $"{{\"{ContentKey}\": \"{contentDateTimeString}\"}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (bool validated, _) = CallDateTimeContentPropertyMatchesNodeProperty();

            Assert.False(validated);
        }

        [Theory]
        [InlineData("content property value was '15/06/2020 14:24:00', but node property value was ''", "")]
        public void DateTimeContentPropertyMatchesNodeProperty_PropertySameTypeButDifferent_ReturnsFailedValidationMessage(string expectedMessage, string nodeValue)
        {
            const string contentDateTimeString = "2020-06-15T14:24:00Z";

            string json = $"{{\"{ContentKey}\": \"{contentDateTimeString}\"}}";
            ContentItemField = JObject.Parse(json);

            SourceNodeProperties.Add(NodePropertyName, nodeValue);

            (_, string message) = CallDateTimeContentPropertyMatchesNodeProperty();

            Assert.Equal(expectedMessage, message);
        }

        //todo: tests for different time zones

        private (bool matched, string failureReason) CallDateTimeContentPropertyMatchesNodeProperty()
        {
            return GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
                ContentKey,
                ContentItemField,
                NodePropertyName,
                SourceNode);
        }
    }
}
