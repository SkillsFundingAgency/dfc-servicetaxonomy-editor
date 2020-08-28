using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.LinkFieldGraphSyncerTests
{
    public class LinkFieldGraphSyncerValidateSyncComponentTestsBase : FieldGraphSyncer_ValidateSyncComponentTestsBase
    {
        public const string ContentKeyText = "Text";
        public const string ContentKeyUrl = "Url";

        public const string FieldNameText = "transformedFieldName_text";
        public const string FieldNameUrl = "transformedFieldName_url";

        public LinkFieldGraphSyncerValidateSyncComponentTestsBase()
        {
            ContentFieldGraphSyncer = new LinkFieldGraphSyncer();
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public async Task ValidateSyncComponentTests(
            bool expected,
            bool urlStringContentPropertyMatchesNodePropertyReturns,
            bool textStringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKeyText,
                A<JObject>._,
                FieldNameText,
                SourceNode)).Returns((textStringContentPropertyMatchesNodePropertyReturns, ""));

            A.CallTo(() => GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKeyUrl,
                A<JObject>._,
                FieldNameUrl,
                SourceNode)).Returns((urlStringContentPropertyMatchesNodePropertyReturns, ""));

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: failure message tests
    }
}
