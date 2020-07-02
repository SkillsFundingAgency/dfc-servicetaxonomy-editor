//todo: reinstate

// using System;
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
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.UnpublishLaterPartGraphSyncerTests
// {
//     public class UnpublishLaterPartGraphSyncer_AddSyncComponentsTests
//     {
//         public JObject Content { get; set; }
//         public ContentItem ContentItem { get; set; }
//         public IMergeNodeCommand MergeNodeCommand { get; set; }
//         public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
//         public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
//         public IContentManager ContentManager { get; set; }
//         public IGraphSyncHelper GraphSyncHelper { get; set; }
//         public UnpublishLaterPartGraphSyncer UnpublishLaterPartGraphSyncer { get; set; }
//
//         public const string NodeTitlePropertyName = "unpublishlater_ScheduledUnpublishUtc";
//
//         public UnpublishLaterPartGraphSyncer_AddSyncComponentsTests()
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
//             UnpublishLaterPartGraphSyncer = new UnpublishLaterPartGraphSyncer();
//         }
//
//         [Fact]
//         public async Task AddSyncComponents_ScheduledUnpublishUtcContent_TitleAddedToMergeNodeCommandsProperties()
//         {
//             const string scheduledDateUtc = "2020-06-28T09:58:00Z";
//
//             Content = JObject.Parse($"{{\"ScheduledUnpublishUtc\": \"{scheduledDateUtc}\"}}");
//
//             await CallAddSyncComponents();
//
//             //Date compare culture is skewed and ends up an hour different
//             IDictionary<string, object> expectedProperties = new Dictionary<string, object>
//                 {{NodeTitlePropertyName, DateTime.Parse(scheduledDateUtc).AddHours(-1)}};
//
//             Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
//         }
//
//         [Fact]
//         public async Task AddSyncComponents_NullScheduledPublicUtcInContent_TitleNotAddedToMergeNodeCommandsProperties()
//         {
//             Content = JObject.Parse("{\"ScheduledUnpublishUtc\": null}");
//
//             await CallAddSyncComponents();
//
//             IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
//             Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
//         }
//
//         private async Task CallAddSyncComponents()
//         {
//             await UnpublishLaterPartGraphSyncer.AddSyncComponents(
//                 Content, TODO);
//         }
//     }
// }
