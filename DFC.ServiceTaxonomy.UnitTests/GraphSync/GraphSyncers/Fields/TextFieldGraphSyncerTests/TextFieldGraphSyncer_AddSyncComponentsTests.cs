//todo: reinstate

// using System.Collections.Generic;
// using System.Threading.Tasks;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
// using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
// using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
// using FakeItEasy;
// using Newtonsoft.Json.Linq;
// using OrchardCore.ContentManagement;
// using Xunit;
//
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.TextFieldGraphSyncerTests
// {
//     public class TextFieldGraphSyncer_AddSyncComponentsTests
//     {
//         public JObject? ContentItemField { get; set; }
//         public IMergeNodeCommand MergeNodeCommand { get; set; }
//         public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
//         //todo: could probably create a real one of these, rather than wrapping a real one
//         public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
//         public IContentManager ContentManager { get; set; }
//         public IGraphSyncHelper GraphSyncHelper { get; set; }
//         public TextFieldGraphSyncer TextFieldGraphSyncer { get; set; }
//
//         const string _fieldName = "TestField";
//
//         public TextFieldGraphSyncer_AddSyncComponentsTests()
//         {
//             MergeNodeCommand = A.Fake<IMergeNodeCommand>();
//             //todo: best way to do this?
//             MergeNodeCommand.Properties = new Dictionary<string, object>();
//
//             ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();
//
//             ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
//             A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);
//
//             ContentManager = A.Fake<IContentManager>();
//
//             GraphSyncHelper = A.Fake<IGraphSyncHelper>();
//             A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);
//
//             TextFieldGraphSyncer = new TextFieldGraphSyncer();
//         }
//
//         [Fact]
//         public async Task AddSyncComponents_TextInContent_TextAddedToMergeNodeCommandsProperties()
//         {
//             const string text = "abc";
//             ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");
//
//             await CallAddSyncComponents();
//
//             IDictionary<string,object> expectedProperties = new Dictionary<string, object>
//                 {{_fieldName, text}};
//
//             Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
//         }
//
//         [Fact]
//         public async Task AddSyncComponents_NullTextInContent_TextNotAddedToMergeNodeCommandsProperties()
//         {
//             ContentItemField = JObject.Parse("{\"Text\": null}");
//
//             await CallAddSyncComponents();
//
//             IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
//             Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
//         }
//
//         //todo: assert that nothing else is done to the commands
//         //todo: assert that graphsynchelper's contenttype is not set
//
//         private async Task CallAddSyncComponents()
//         {
//             await TextFieldGraphSyncer.AddSyncComponents(
//                 ContentItemField!, TODO);
//         }
//     }
// }
