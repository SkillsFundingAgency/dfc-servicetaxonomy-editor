//todo: reinstate

// using System.Collections.Generic;
// using System.Threading.Tasks;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
// using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
// using FakeItEasy;
// using Newtonsoft.Json.Linq;
// using OrchardCore.ContentManagement;
// using OrchardCore.ContentManagement.Metadata.Models;
// using Xunit;
//
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.AliasPartGraphSyncerTests
// {
//     public class AliasPartGraphSyncer_AddSyncComponentsTests
//     {
//         public JObject Content { get; set; }
//         public ContentItem ContentItem { get; set; }
//         public IContentManager ContentManager { get; set; }
//         public IMergeNodeCommand MergeNodeCommand { get; set; }
//         public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
//         public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
//         public IGraphSyncHelper GraphSyncHelper { get; set; }
//         public AliasPartGraphSyncer AliasPartGraphSyncer { get; set; }
//
//         public const string NodeTitlePropertyName = "alias_alias";
//
//         public AliasPartGraphSyncer_AddSyncComponentsTests()
//         {
//             MergeNodeCommand = A.Fake<IMergeNodeCommand>();
//             //todo: best way to do this?
//             MergeNodeCommand.Properties = new Dictionary<string, object>();
//
//             ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();
//             ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();
//             GraphSyncHelper = A.Fake<IGraphSyncHelper>();
//
//             Content = JObject.Parse("{}");
//             ContentItem = A.Fake<ContentItem>();
//             ContentManager = A.Fake<IContentManager>();
//
//             AliasPartGraphSyncer = new AliasPartGraphSyncer();
//         }
//
//         [Fact]
//         public async Task AddSyncComponents_TitleInContent_TitleAddedToMergeNodeCommandsProperties()
//         {
//             const string alias = "alias";
//
//             Content = JObject.Parse($"{{\"Alias\": \"{alias}\"}}");
//
//             await CallAddSyncComponents();
//
//             IDictionary<string,object> expectedProperties = new Dictionary<string, object>
//                 {{NodeTitlePropertyName, alias}};
//
//             Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
//         }
//
//         [Fact]
//         public async Task AddSyncComponents_NullTitleInContent_TitleNotAddedToMergeNodeCommandsProperties()
//         {
//             Content = JObject.Parse("{\"Alias\": null}");
//
//             await CallAddSyncComponents();
//
//             IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
//             Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
//         }
//
//         private async Task CallAddSyncComponents()
//         {
//             await AliasPartGraphSyncer.AddSyncComponents(
//                 Content, TODO);
//         }
//     }
// }
