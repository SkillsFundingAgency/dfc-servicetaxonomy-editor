using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.GraphSyncPartGraphSyncerTests
{
    public class GraphSyncPartGraphSyncerValidateSyncComponentTestsBase : PartGraphSyncer_ValidateSyncComponentTestsBase
    {
        public const string ContentIdPropertyName = "Text";
        public const string NodeTitlePropertyName = "skos__prefLabel";

        public GraphSyncPartGraphSyncerValidateSyncComponentTestsBase()
        {
            A.CallTo(() => SyncNameProvider.ContentIdPropertyName).Returns(ContentIdPropertyName);
            A.CallTo(() => SyncNameProvider.IdPropertyName()).Returns(NodeTitlePropertyName);

            ContentPartGraphSyncer = new DataSyncPartDataSyncer();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ValidateSyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => DataSyncValidationHelper.ContentPropertyMatchesNodeProperty(
                ContentIdPropertyName,
                A<JObject>._,
                NodeTitlePropertyName,
                SourceNode,
                A<Func<JValue, object, bool>>._)).Returns((stringContentPropertyMatchesNodePropertyReturns, ""));

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: test that verifies that failure reason is returned
        //todo: test to check nothing added to ExpectedRelationshipCounts
    }
}
