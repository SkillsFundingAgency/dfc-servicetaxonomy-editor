using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.HtmlBodyPartGraphSyncerTests
{
    public class HtmlBodyPartGraphSyncerValidateSyncComponentTestsBase : PartGraphSyncer_ValidateSyncComponentTestsBase
    {
        public const string ContentKey = "Html";
        public const string NodeTitlePropertyName = "htmlbody_Html";

        public HtmlBodyPartGraphSyncerValidateSyncComponentTestsBase()
        {
            ContentPartGraphSyncer = new HtmlBodyPartGraphSyncer();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ValidateSyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKey,
                A<JsonObject>._,
                NodeTitlePropertyName,
                SourceNode)).Returns((stringContentPropertyMatchesNodePropertyReturns, ""));
            A.CallTo(() => SyncNameProvider.PropertyName("Html")).Returns("htmlbody_Html");

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: test that verifies that failure reason is returned
        //todo: test to check nothing added to ExpectedRelationshipCounts
    }
}
