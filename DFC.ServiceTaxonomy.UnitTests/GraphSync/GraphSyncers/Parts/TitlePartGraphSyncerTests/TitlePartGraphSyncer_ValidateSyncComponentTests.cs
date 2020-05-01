using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.TitlePartGraphSyncerTests
{
    public class TitlePartGraphSyncer_ValidateSyncComponentTests
    {
        public JObject Content { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; set; }
        public INode SourceNode { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public IGraphValidationHelper GraphValidationHelper { get; set; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
        public TitlePartGraphSyncer TitlePartGraphSyncer { get; set; }

        const string _contentKey = "Title";
        const string _nodeTitlePropertyName = "skos__prefLabel";

        public TitlePartGraphSyncer_ValidateSyncComponentTests()
        {
            Content = JObject.Parse("{}");

            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();

            SourceNode = A.Fake<INode>();
            NodeWithOutgoingRelationships = A.Fake<INodeWithOutgoingRelationships>();
            A.CallTo(() => NodeWithOutgoingRelationships.SourceNode).Returns(SourceNode);

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();

            GraphValidationHelper = A.Fake<IGraphValidationHelper>();

            ExpectedRelationshipCounts = new Dictionary<string, int>();

            TitlePartGraphSyncer = new TitlePartGraphSyncer();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ValidateSyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                _contentKey,
                A<JObject>._,
                _nodeTitlePropertyName,
                SourceNode)).Returns((stringContentPropertyMatchesNodePropertyReturns, ""));

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: test that verifies that failure reason is returned
        //todo: test to check nothing added to ExpectedRelationshipCounts

        private async Task<(bool validated, string failureReason)> CallValidateSyncComponent()
        {
            return await TitlePartGraphSyncer.ValidateSyncComponent(
                Content,
                ContentTypePartDefinition,
                NodeWithOutgoingRelationships,
                GraphSyncHelper,
                GraphValidationHelper,
                ExpectedRelationshipCounts,
                string.Empty);
        }
    }
}
