using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Fields;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.HtmlFieldGraphSyncerTests
{
    public class HtmlFieldGraphSyncerValidateSyncComponentTestsBase : FieldGraphSyncer_ValidateSyncComponentTestsBase
    {
        const string ContentKey = "Html";

        public HtmlFieldGraphSyncerValidateSyncComponentTestsBase()
        {
            ContentFieldGraphSyncer = new HtmlFieldDataSyncer();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ValidateSyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKey,
                A<JObject>._,
                FieldNameTransformed,
                SourceNode)).Returns((stringContentPropertyMatchesNodePropertyReturns, ""));

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: test that verifies that failure reason is returned
    }
}
