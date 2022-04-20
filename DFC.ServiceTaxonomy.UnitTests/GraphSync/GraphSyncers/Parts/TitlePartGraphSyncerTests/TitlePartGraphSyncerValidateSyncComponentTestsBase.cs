using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts;
using DFC.ServiceTaxonomy.DataSync.Services;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.TitlePartGraphSyncerTests
{
    public class TitlePartGraphSyncerValidateSyncComponentTestsBase : PartGraphSyncer_ValidateSyncComponentTestsBase
    {
        public const string ContentKey = "Title";
        public const string NodeTitlePropertyName = "skos__prefLabel";

        public TitlePartGraphSyncerValidateSyncComponentTestsBase()
        {
            ContentPartGraphSyncer = new TitlePartDataSyncer(new TitlePartCloneGenerator());
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

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: test that verifies that failure reason is returned
        //todo: test to check nothing added to ExpectedRelationshipCounts
    }
}
