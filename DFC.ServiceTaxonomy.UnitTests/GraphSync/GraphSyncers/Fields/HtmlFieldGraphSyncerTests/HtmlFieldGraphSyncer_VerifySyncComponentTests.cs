using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.HtmlFieldGraphSyncerTests
{
    public class HtmlFieldGraphSyncer_VerifySyncComponentTests
    {
        public JObject ContentItemField { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; set; }
        public INode SourceNode { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public IGraphValidationHelper GraphValidationHelper { get; set; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
        public HtmlFieldGraphSyncer HtmlFieldGraphSyncer { get; set; }

        const string _contentKey = "Html";
        const string _fieldNameBase = "fieldNameBase";
        const string _fieldNameTransformed = "fieldNameTransformed";

        public HtmlFieldGraphSyncer_VerifySyncComponentTests()
        {
            ContentItemField = JObject.Parse("{}");

            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldNameBase);

            SourceNode = A.Fake<INode>();

            NodeWithOutgoingRelationships = A.Fake<INodeWithOutgoingRelationships>();
            A.CallTo(() => NodeWithOutgoingRelationships.SourceNode).Returns(SourceNode);

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName(_fieldNameBase)).Returns(_fieldNameTransformed);

            GraphValidationHelper = A.Fake<IGraphValidationHelper>();

            ExpectedRelationshipCounts = new Dictionary<string, int>();

            HtmlFieldGraphSyncer = new HtmlFieldGraphSyncer();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task VerifySyncComponentTests(bool expected, bool stringContentPropertyMatchesNodePropertyReturns)
        {
            A.CallTo(() => GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                _contentKey,
                A<JObject>._,
                _fieldNameTransformed,
                SourceNode)).Returns((stringContentPropertyMatchesNodePropertyReturns, ""));

            (bool verified, _) = await CallVerifySyncComponent();

            Assert.Equal(expected, verified);
        }

        //todo: test that verifies that failure reason is returned

        private async Task<(bool verified, string failureReason)> CallVerifySyncComponent()
        {
            return await HtmlFieldGraphSyncer.VerifySyncComponent(
                ContentItemField,
                ContentPartFieldDefinition,
                NodeWithOutgoingRelationships,
                GraphSyncHelper,
                GraphValidationHelper,
                ExpectedRelationshipCounts);
        }
    }
}
