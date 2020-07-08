using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.PartGraphSyncer;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.AutoroutePartGraphSyncerTests
{
    public class AutoroutePartGraphSyncer_ValidateSyncComponentTests : PartGraphSyncer_ValidateSyncComponentTests
    {
        const string ContentKey = "Path";
        const string NodeTitlePropertyName = "autoroute_path";

        public AutoroutePartGraphSyncer_ValidateSyncComponentTests()
        {
            ContentPartGraphSyncer = new AutoroutePartGraphSyncer();
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

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: test that verifies that failure reason is returned
        //todo: test to check nothing added to ExpectedRelationshipCounts
    }
}
