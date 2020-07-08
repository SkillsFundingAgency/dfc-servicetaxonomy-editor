using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.PartGraphSyncer;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.HtmlBodyPartGraphSyncerTests
{
    public class PublishLaterPartGraphSyncer_ValidateSyncComponentTests : PartGraphSyncer_ValidateSyncComponentTests
    {
        const string ContentKey = "ScheduledPublishUtc";
        const string NodeTitlePropertyName = "publishlater_ScheduledPublishUtc";

        public PublishLaterPartGraphSyncer_ValidateSyncComponentTests()
        {
            ContentPartGraphSyncer = new PublishLaterPartGraphSyncer();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ValidateSyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
                ContentKey,
                A<JObject>._,
                NodeTitlePropertyName,
                SourceNode)).Returns((stringContentPropertyMatchesNodePropertyReturns, ""));
            A.CallTo(() => GraphSyncHelper.PropertyName("ScheduledPublishUtc")).Returns("publishlater_ScheduledPublishUtc");

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: test that verifies that failure reason is returned
        //todo: test to check nothing added to ExpectedRelationshipCounts
    }
}
