using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.HtmlFieldGraphSyncerTests
{
    public class HtmlFieldGraphSyncer_VerifySyncComponentTests
    {
        public JObject? ContentItemField { get; set; }
        public ContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public IEnumerable<IRelationship> Relationships { get; set; }
        public IEnumerable<INode> DestinationNodes { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public HtmlFieldGraphSyncer HtmlFieldGraphSyncer { get; set; }

        const string _fieldName = "TestHtmlFieldName";

        public HtmlFieldGraphSyncer_VerifySyncComponentTests()
        {
            //todo: if this ok, remove interface
            //ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            //A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);
            ContentPartFieldDefinition = new ContentPartFieldDefinition(null, _fieldName, null);

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);

            Relationships = new IRelationship[0];
            DestinationNodes = new INode[0];

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);

            HtmlFieldGraphSyncer = new HtmlFieldGraphSyncer();
        }

        [Fact]
        public async Task VerifySyncComponent_PropertyCorrect_ReturnsTrue()
        {
            const string html = "<p>abc</p>";
            ContentItemField = JObject.Parse($"{{\"Html\": \"{html}\"}}");

            SourceNodeProperties.Add(_fieldName, html);

            bool verified = await CallVerifySyncComponent();

            Assert.True(verified);
        }

        // no need to check for missing source node, that'll be handled in ValidateGraphSync

        [Fact]
        public async Task VerifySyncComponent_PropertyMissing_ReturnsFalse()
        {
            const string html = "<p>abc</p>";
            ContentItemField = JObject.Parse($"{{\"Html\": \"{html}\"}}");

            bool verified = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_PropertyDifferent_ReturnsFalse()
        {
            const string html = "<p>abc</p>";
            ContentItemField = JObject.Parse($"{{\"Html\": \"{html}\"}}");

            SourceNodeProperties.Add(_fieldName, "some_other_value");

            bool verified = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        private async Task<bool> CallVerifySyncComponent()
        {
            return await HtmlFieldGraphSyncer.VerifySyncComponent(
                ContentItemField!,
                ContentPartFieldDefinition,
                SourceNode,
                Relationships,
                DestinationNodes,
                GraphSyncHelper);
        }
    }
}
