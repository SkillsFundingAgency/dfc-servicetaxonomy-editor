using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.UnpublishLaterPartGraphSyncerTests
{
    public class UnpublishLaterPartGraphSyncerValidateSyncComponentTestsBase : PartGraphSyncer_ValidateSyncComponentTestsBase
    {
        public const string ContentKey = "ScheduledUnpublishUtc";
        public const string NodeTitlePropertyName = "unpublishlater_ScheduledUnpublishUtc";

        public UnpublishLaterPartGraphSyncerValidateSyncComponentTestsBase()
        {
            ContentPartGraphSyncer = new UnpublishLaterPartDataSyncer();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ValidateSyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => DataSyncValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
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
