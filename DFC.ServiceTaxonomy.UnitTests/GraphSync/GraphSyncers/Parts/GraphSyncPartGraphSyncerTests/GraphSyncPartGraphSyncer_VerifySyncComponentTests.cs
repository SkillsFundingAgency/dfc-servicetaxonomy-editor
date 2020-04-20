// using System.Collections.Generic;
// using System.Threading.Tasks;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
// using FakeItEasy;
// using Neo4j.Driver;
// using Newtonsoft.Json.Linq;
// using OrchardCore.ContentManagement.Metadata.Models;
// using Xunit;
//
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.GraphSyncPartGraphSyncerTests
// {
//     public class GraphSyncPartGraphSyncer_VerifySyncComponentTests
//     {
//         public JObject Content { get; set; }
//         public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
//         public INode SourceNode { get; set; }
//         public IEnumerable<IRelationship> Relationships { get; set; }
//         public IEnumerable<INode> DestinationNodes { get; set; }
//         public IGraphSyncHelper GraphSyncHelper { get; set; }
//         public IGraphValidationHelper GraphValidationHelper { get; set; }
//         public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
//         public GraphSyncPartGraphSyncer GraphSyncPartGraphSyncer { get; set; }
//
//         const string _contentIdPropertyName = "Text";
//         const string _nodeTitlePropertyName = "skos__prefLabel";
//
//         public GraphSyncPartGraphSyncer_VerifySyncComponentTests()
//         {
//             Content = JObject.Parse("{}");
//
//             ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();
//
//             SourceNode = A.Fake<INode>();
//
//             Relationships = new IRelationship[0];
//             DestinationNodes = new INode[0];
//
//             GraphSyncHelper = A.Fake<IGraphSyncHelper>();
//             A.CallTo(() => GraphSyncHelper.ContentIdPropertyName).Returns(_contentIdPropertyName);
//             A.CallTo(() => GraphSyncHelper.IdPropertyName()).Returns(_nodeTitlePropertyName);
//
//             GraphValidationHelper = A.Fake<IGraphValidationHelper>();
//
//             ExpectedRelationshipCounts = new Dictionary<string, int>();
//
//             GraphSyncPartGraphSyncer = new GraphSyncPartGraphSyncer();
//         }
//
//         [Theory]
//         [InlineData(true, true)]
//         [InlineData(false, false)]
//         public async Task VerifySyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
//         {
//             A.CallTo(() => GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
//                 _contentIdPropertyName,
//                 A<JObject>._,
//                 _nodeTitlePropertyName,
//                 SourceNode)).Returns((stringContentPropertyMatchesNodePropertyReturns, ""));
//
//             (bool verified, _) = await CallVerifySyncComponent();
//
//             Assert.Equal(expected, verified);
//         }
//
//         //todo: test that verifies that failure reason is returned
//         //todo: test to check nothing added to ExpectedRelationshipCounts
//
//         private async Task<(bool verified, string failureReason)> CallVerifySyncComponent()
//         {
//             return await GraphSyncPartGraphSyncer.VerifySyncComponent(
//                 Content,
//                 ContentTypePartDefinition, TODO,
//                 GraphSyncHelper,
//                 GraphValidationHelper,
//                 ExpectedRelationshipCounts);
//         }
//     }
// }
