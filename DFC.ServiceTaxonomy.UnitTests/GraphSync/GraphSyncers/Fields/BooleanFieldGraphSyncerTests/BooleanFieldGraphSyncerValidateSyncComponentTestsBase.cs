// using System.Threading.Tasks;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
// using FakeItEasy;
// using Newtonsoft.Json.Linq;
// using Xunit;
//
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.BooleanFieldGraphSyncerTests
// {
//     public class BooleanFieldGraphSyncerValidateSyncComponentTestsBase : FieldGraphSyncer_ValidateSyncComponentTestsBase
//     {
//         private const string ContentKey = "Value";
//
//         public BooleanFieldGraphSyncerValidateSyncComponentTestsBase()
//         {
//             ContentFieldGraphSyncer = new BooleanFieldGraphSyncer();
//         }
//
//         [Theory]
//         [InlineData(true)]
//         [InlineData(false)]
//         public async Task ValidateSyncComponentTests(bool boolMatches)
//         {
//             A.CallTo(() => GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
//                 ContentKey,
//                 A<JObject>._,
//                 FieldNameTransformed,
//                 SourceNode)).Returns((boolMatches, ""));
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.Equal(boolMatches, validated);
//         }
//     }
// }
