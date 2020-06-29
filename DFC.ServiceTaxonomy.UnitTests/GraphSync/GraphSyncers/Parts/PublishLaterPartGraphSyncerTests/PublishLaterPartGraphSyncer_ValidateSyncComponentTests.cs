using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.HtmlBodyPartGraphSyncerTests
{
    public class PublishLaterPartGraphSyncer_ValidateSyncComponentTests
    {
        public JObject Content { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentManager ContentManager { get; set; }
        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; set; }
        public INode SourceNode { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public IGraphValidationHelper GraphValidationHelper { get; set; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
        public PublishLaterPartGraphSyncer PublishLaterPartGraphSyncer { get; set; }

        const string _contentKey = "ScheduledPublishUtc";
        const string _nodeTitlePropertyName = "publishlater_ScheduledPublishUtc";

        public PublishLaterPartGraphSyncer_ValidateSyncComponentTests()
        {
            Content = JObject.Parse("{}");
            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();
            ContentManager = A.Fake<IContentManager>();

            SourceNode = A.Fake<INode>();
            NodeWithOutgoingRelationships = A.Fake<INodeWithOutgoingRelationships>();
            A.CallTo(() => NodeWithOutgoingRelationships.SourceNode).Returns(SourceNode);

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();

            GraphValidationHelper = A.Fake<IGraphValidationHelper>();

            ExpectedRelationshipCounts = new Dictionary<string, int>();

            PublishLaterPartGraphSyncer = new PublishLaterPartGraphSyncer();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ValidateSyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => GraphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
                _contentKey,
                A<JObject>._,
                _nodeTitlePropertyName,
                SourceNode)).Returns((stringContentPropertyMatchesNodePropertyReturns, ""));
            A.CallTo(() => GraphSyncHelper.PropertyName("ScheduledPublishUtc")).Returns("publishlater_ScheduledPublishUtc");

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: test that verifies that failure reason is returned
        //todo: test to check nothing added to ExpectedRelationshipCounts

        private async Task<(bool validated, string failureReason)> CallValidateSyncComponent()
        {
            return await PublishLaterPartGraphSyncer.ValidateSyncComponent(
                Content,
                ContentTypePartDefinition,
                ContentManager,
                NodeWithOutgoingRelationships,
                GraphSyncHelper,
                GraphValidationHelper,
                ExpectedRelationshipCounts,
                string.Empty);
        }
    }
}
