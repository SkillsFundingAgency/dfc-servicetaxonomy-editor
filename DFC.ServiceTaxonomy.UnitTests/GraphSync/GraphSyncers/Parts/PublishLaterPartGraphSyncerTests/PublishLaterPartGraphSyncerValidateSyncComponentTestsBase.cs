// using System.Threading.Tasks;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
// using FakeItEasy;
// using Newtonsoft.Json.Linq;
// using Xunit;
//
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.HtmlBodyPartGraphSyncerTests
// {
//     public class PublishLaterPartGraphSyncerValidateSyncComponentTestsBase : PartGraphSyncer_ValidateSyncComponentTestsBase
//     {
//         public const string ContentKey = "ScheduledPublishUtc";
//         public const string NodeTitlePropertyName = "publishlater_ScheduledPublishUtc";
//
//         public PublishLaterPartGraphSyncerValidateSyncComponentTestsBase()
//         {
//             ContentPartGraphSyncer = new PublishLaterPartGraphSyncer();
//         }
//
//         [Theory]
//         [InlineData(true, true)]
//         [InlineData(false, false)]
//         public async Task ValidateSyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
//         {
//             A.CallTo(() => GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
//                 ContentKey,
//                 A<JObject>._,
//                 NodeTitlePropertyName,
//                 SourceNode)).Returns((stringContentPropertyMatchesNodePropertyReturns, ""));
//             A.CallTo(() => SyncNameProvider.PropertyName("ScheduledPublishUtc")).Returns("publishlater_ScheduledPublishUtc");
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.Equal(expected, validated);
//         }
//
//         //todo: test that verifies that failure reason is returned
//         //todo: test to check nothing added to ExpectedRelationshipCounts
//     }
// }
