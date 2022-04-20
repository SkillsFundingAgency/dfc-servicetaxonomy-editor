using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.HtmlBodyPartGraphSyncerTests
{
    public class HtmlBodyPartGraphSyncerValidateSyncComponentTestsBase : PartGraphSyncer_ValidateSyncComponentTestsBase
    {
        public const string ContentKey = "Html";
        public const string NodeTitlePropertyName = "htmlbody_Html";

        public HtmlBodyPartGraphSyncerValidateSyncComponentTestsBase()
        {
            ContentPartGraphSyncer = new HtmlBodyPartDataSyncer();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ValidateSyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKey,
                A<JObject>._,
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
