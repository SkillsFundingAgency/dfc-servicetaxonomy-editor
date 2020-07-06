//todo: reinstate

// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
// using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
// using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
// using FakeItEasy;
// using Neo4j.Driver;
// using Newtonsoft.Json.Linq;
// using OrchardCore.ContentManagement;
// using Xunit;
//
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.DateTimeFieldGraphSyncerTests
// {
//     public class DateTimeFieldGraphSyncer_ValidateSyncComponentTests
//     {
//         public JObject? ContentItemField { get; set; }
//         public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
//         public IContentManager ContentManager { get; set; }
//         public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; set; }
//         public INode SourceNode { get; set; }
//         public Dictionary<string, object> SourceNodeProperties { get; set; }
//         public IGraphSyncHelper GraphSyncHelper { get; set; }
//         public IGraphValidationHelper GraphValidationHelper { get; set; }
//         public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
//         public DateTimeFieldGraphSyncer DateTimeFieldGraphSyncer { get; set; }
//
//         const string _fieldName = "TestDateTimeFieldName";
//         const string _contentKey = "Value";
//
//         public DateTimeFieldGraphSyncer_ValidateSyncComponentTests()
//         {
//             ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
//             A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);
//
//             ContentManager = A.Fake<IContentManager>();
//
//             SourceNode = A.Fake<INode>();
//             SourceNodeProperties = new Dictionary<string, object>();
//             A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);
//             NodeWithOutgoingRelationships = A.Fake<INodeWithOutgoingRelationships>();
//             A.CallTo(() => NodeWithOutgoingRelationships.SourceNode).Returns(SourceNode);
//
//             GraphSyncHelper = A.Fake<IGraphSyncHelper>();
//             A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);
//
//             GraphValidationHelper = A.Fake<IGraphValidationHelper>();
//
//             ExpectedRelationshipCounts = new Dictionary<string, int>();
//
//             DateTimeFieldGraphSyncer = new DateTimeFieldGraphSyncer();
//         }
//
//         [Theory]
//         [InlineData(true)]
//         [InlineData(false)]
//         public async Task ValidateSyncComponent_DateTimePropertyCorrect_ReturnsTrue(bool dateTimeMatches)
//         {
//             A.CallTo(() => GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
//                 _contentKey,
//                 A<JObject>._,
//                 A<string>.Ignored,
//                 SourceNode)).Returns((dateTimeMatches, ""));
//
//             DateTime value = new DateTime(2019, 11, 12, 15, 34, 01);
//
//             ContentItemField = JObject.Parse($"{{\"Value\": \"{value:o}\"}}");
//
//             SourceNodeProperties.Add(_fieldName, value);
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.Equal(dateTimeMatches, validated);
//         }
//
//         private async Task<(bool validated, string failureReason)> CallValidateSyncComponent()
//         {
//             return await DateTimeFieldGraphSyncer.ValidateSyncComponent(
//                 ContentItemField!, TODO);
//         }
//     }
// }
