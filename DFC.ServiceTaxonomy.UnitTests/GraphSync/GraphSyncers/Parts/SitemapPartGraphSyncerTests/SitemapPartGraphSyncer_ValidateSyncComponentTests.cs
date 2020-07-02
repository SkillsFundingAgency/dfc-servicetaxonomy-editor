//todo: reinstate

// using System.Collections.Generic;
// using System.Threading.Tasks;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
// using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
// using FakeItEasy;
// using Neo4j.Driver;
// using Newtonsoft.Json.Linq;
// using OrchardCore.ContentManagement;
// using OrchardCore.ContentManagement.Metadata.Models;
// using OrchardCore.Sitemaps.Models;
// using Xunit;
//
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.SitemapPartGraphSyncerTests
// {
//     public class SitemapPartGraphSyncer_ValidateSyncComponentTests
//     {
//         public JObject Content { get; set; }
//         public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
//         public IContentManager ContentManager { get; set; }
//         public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; set; }
//         public INode SourceNode { get; set; }
//         public IGraphSyncHelper GraphSyncHelper { get; set; }
//         public IGraphValidationHelper GraphValidationHelper { get; set; }
//         public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
//         public SitemapPartGraphSyncer SitemapPartGraphSyncer { get; set; }
//
//         public SitemapPartGraphSyncer_ValidateSyncComponentTests()
//         {
//             Content = JObject.Parse("{}");
//             ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();
//             ContentManager = A.Fake<IContentManager>();
//
//             SourceNode = A.Fake<INode>();
//             NodeWithOutgoingRelationships = A.Fake<INodeWithOutgoingRelationships>();
//             A.CallTo(() => NodeWithOutgoingRelationships.SourceNode).Returns(SourceNode);
//
//             GraphSyncHelper = A.Fake<IGraphSyncHelper>();
//             A.CallTo(() => GraphSyncHelper.PropertyName("OverrideSitemapConfig")).Returns("sitemap_OverrideSitemapConfig");
//             A.CallTo(() => GraphSyncHelper.PropertyName("ChangeFrequency")).Returns("sitemap_ChangeFrequency");
//             A.CallTo(() => GraphSyncHelper.PropertyName("Priority")).Returns("sitemap_Priority");
//             A.CallTo(() => GraphSyncHelper.PropertyName("Exclude")).Returns("sitemap_Exclude");
//
//             GraphValidationHelper = A.Fake<IGraphValidationHelper>();
//
//             ExpectedRelationshipCounts = new Dictionary<string, int>();
//
//             SitemapPartGraphSyncer = new SitemapPartGraphSyncer();
//         }
//
//         [Theory]
//         [InlineData(true, true, true, true, true)]
//         [InlineData(false, true, true, true, false)]
//         [InlineData(false, true, true, false, true)]
//         [InlineData(false, true, false, true, true)]
//         [InlineData(false, false, true, true, true)]
//         [InlineData(false, false, false, false, false)]
//         public async Task ValidateSyncComponentTests(bool expected, bool overrideSitemapConfigMatches, bool changeFrequencyMatches, bool priorityMatches, bool exludeMatches)
//         {
//             A.CallTo(() => GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
//                 "OverrideSitemapConfig",
//                 A<JObject>._,
//                 "sitemap_OverrideSitemapConfig",
//                 SourceNode)).Returns((overrideSitemapConfigMatches, ""));
//
//             A.CallTo(() => GraphValidationHelper.EnumContentPropertyMatchesNodeProperty<ChangeFrequency>(
//                 "ChangeFrequency",
//                 A<JObject>._,
//                 "sitemap_ChangeFrequency",
//                 SourceNode)).Returns((changeFrequencyMatches, ""));
//
//             A.CallTo(() => GraphValidationHelper.LongContentPropertyMatchesNodeProperty(
//                 "Priority",
//                 A<JObject>._,
//                 "sitemap_Priority",
//                 SourceNode)).Returns((priorityMatches, ""));
//
//             A.CallTo(() => GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
//                 "Exclude",
//                 A<JObject>._,
//                 "sitemap_Exclude",
//                 SourceNode)).Returns((exludeMatches, ""));
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.Equal(expected, validated);
//         }
//
//         //todo: test that verifies that failure reason is returned
//         //todo: test to check nothing added to ExpectedRelationshipCounts
//
//         private async Task<(bool validated, string failureReason)> CallValidateSyncComponent()
//         {
//             return await SitemapPartGraphSyncer.ValidateSyncComponent(
//                 Content,
//                 ContentTypePartDefinition,
//                 ContentManager,
//                 NodeWithOutgoingRelationships,
//                 GraphSyncHelper,
//                 GraphValidationHelper,
//                 ExpectedRelationshipCounts, TODO);
//         }
//     }
// }
