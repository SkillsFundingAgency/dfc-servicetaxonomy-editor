using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.PartGraphSyncer;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.GraphSyncPartGraphSyncerTests
{
    public class GraphSyncPartGraphSyncer_ValidateSyncComponentTests : PartGraphSyncer_ValidateSyncComponentTests
    {
        const string ContentIdPropertyName = "Text";
        const string NodeTitlePropertyName = "skos__prefLabel";

        public GraphSyncPartGraphSyncer_ValidateSyncComponentTests()
        {
            A.CallTo(() => GraphSyncHelper.ContentIdPropertyName).Returns(ContentIdPropertyName);
            A.CallTo(() => GraphSyncHelper.IdPropertyName()).Returns(NodeTitlePropertyName);

            ContentPartGraphSyncer = new GraphSyncPartGraphSyncer();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ValidateSyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentIdPropertyName,
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
