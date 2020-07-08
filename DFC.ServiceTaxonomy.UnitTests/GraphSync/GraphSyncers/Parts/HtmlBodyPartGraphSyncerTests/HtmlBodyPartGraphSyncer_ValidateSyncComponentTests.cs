using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.PartGraphSyncer;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.HtmlBodyPartGraphSyncerTests
{
    public class HtmlBodyPartGraphSyncer_ValidateSyncComponentTests : PartGraphSyncer_ValidateSyncComponentTests
    {
        const string ContentKey = "Html";
        const string NodeTitlePropertyName = "htmlbody_Html";

        public HtmlBodyPartGraphSyncer_ValidateSyncComponentTests()
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
                A<JObject>._,
                NodeTitlePropertyName,
                SourceNode)).Returns((stringContentPropertyMatchesNodePropertyReturns, ""));
            A.CallTo(() => GraphSyncHelper.PropertyName("Html")).Returns("htmlbody_Html");

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: test that verifies that failure reason is returned
        //todo: test to check nothing added to ExpectedRelationshipCounts
    }
}
