//todo: reinstate

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
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.LinkFieldGraphSyncerTests
// {
//     public class LinkFieldGraphSyncer_ValidateSyncComponentTests
//     {
//         public JObject ContentItemField { get; set; }
//         public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
//         public IContentManager ContentManager { get; set; }
//         public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; set; }
//         public INode SourceNode { get; set; }
//         public IGraphSyncHelper GraphSyncHelper { get; set; }
//         public IGraphValidationHelper GraphValidationHelper { get; set; }
//         public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
//         public LinkFieldGraphSyncer LinkFieldGraphSyncer { get; set; }
//
//         const string _contentKeyText = "Text";
//         const string _contentKeyUrl = "Url";
//
//         const string _fieldNameBase = "baseFieldName";
//         const string _fieldNameTransformed = "transformedFieldName";
//         const string _fieldNameText = "transformedFieldName_text";
//         const string _fieldNameUrl = "transformedFieldName_url";
//
//         public LinkFieldGraphSyncer_ValidateSyncComponentTests()
//         {
//             ContentItemField = JObject.Parse("{}");
//
//             ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
//             A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldNameBase);
//
//             ContentManager = A.Fake<IContentManager>();
//
//             SourceNode = A.Fake<INode>();
//             NodeWithOutgoingRelationships = A.Fake<INodeWithOutgoingRelationships>();
//             A.CallTo(() => NodeWithOutgoingRelationships.SourceNode).Returns(SourceNode);
//
//             GraphSyncHelper = A.Fake<IGraphSyncHelper>();
//             A.CallTo(() => GraphSyncHelper.PropertyName(_fieldNameBase)).Returns(_fieldNameTransformed);
//
//             GraphValidationHelper = A.Fake<IGraphValidationHelper>();
//
//             ExpectedRelationshipCounts = new Dictionary<string, int>();
//
//             LinkFieldGraphSyncer = new LinkFieldGraphSyncer();
//         }
//
//         [Theory]
//         [InlineData(true, true, true)]
//         [InlineData(false, true, false)]
//         [InlineData(false, false, true)]
//         [InlineData(false, false, false)]
//         public async Task ValidateSyncComponentTests(
//             bool expected,
//             bool urlStringContentPropertyMatchesNodePropertyReturns,
//             bool textStringContentPropertyMatchesNodePropertyReturns)
//         {
//             A.CallTo(() => GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
//                 _contentKeyText,
//                 A<JObject>._,
//                 _fieldNameText,
//                 SourceNode)).Returns((textStringContentPropertyMatchesNodePropertyReturns, ""));
//
//             A.CallTo(() => GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
//                 _contentKeyUrl,
//                 A<JObject>._,
//                 _fieldNameUrl,
//                 SourceNode)).Returns((urlStringContentPropertyMatchesNodePropertyReturns, ""));
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.Equal(expected, validated);
//         }
//
//         //todo: failure message tests
//
//         private async Task<(bool validated, string failureReason)> CallValidateSyncComponent()
//         {
//             return await LinkFieldGraphSyncer.ValidateSyncComponent(
//                 ContentItemField, TODO);
//         }
//     }
// }
