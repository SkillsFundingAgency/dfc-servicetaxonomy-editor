// using System.Threading.Tasks;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
// using FakeItEasy;
// using Newtonsoft.Json.Linq;
// using Xunit;
//
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.DateTimeFieldGraphSyncerTests
// {
//     public class DateTimeFieldGraphSyncerValidateSyncComponentTestsBase : FieldGraphSyncer_ValidateSyncComponentTestsBase
//     {
//         private const string ContentKey = "Value";
//
//         public DateTimeFieldGraphSyncerValidateSyncComponentTestsBase()
//         {
//             ContentFieldGraphSyncer = new DateTimeFieldGraphSyncer();
//         }
//
//         //todo: move into base?
//         [Theory]
//         [InlineData(true)]
//         [InlineData(false)]
//         public async Task ValidateSyncComponentTests(bool dateTimeMatches)
//         {
//             A.CallTo(() => GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
//                 ContentKey,
//                 A<JObject>._,
//                 FieldNameTransformed,
//                 SourceNode)).Returns((dateTimeMatches, ""));
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.Equal(dateTimeMatches, validated);
//         }
//     }
// }
