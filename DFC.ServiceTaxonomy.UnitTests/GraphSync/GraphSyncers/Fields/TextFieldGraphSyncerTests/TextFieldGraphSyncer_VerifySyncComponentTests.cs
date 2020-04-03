using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.TextFieldGraphSyncerTests
{
    public class TextFieldGraphSyncer_VerifySyncComponentTests
    {
        public JObject? ContentItemField { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public IEnumerable<IRelationship> Relationships { get; set; }
        public IEnumerable<INode> DestinationNodes { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public TextFieldGraphSyncer TextFieldGraphSyncer { get; set; }

        const string _fieldName = "TestTextFieldName";

        public TextFieldGraphSyncer_VerifySyncComponentTests()
        {
            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);

            SourceNode = A.Fake<INode>();
            SourceNodeProperties = new Dictionary<string, object>();
            A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);

            Relationships = new IRelationship[0];
            DestinationNodes = new INode[0];

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);

            TextFieldGraphSyncer = new TextFieldGraphSyncer();
        }

        [Fact]
        public async Task VerifySyncComponent_PropertyCorrect_ReturnsTrue()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            SourceNodeProperties.Add(_fieldName, text);

            bool verified = await CallVerifySyncComponent();

            Assert.True(verified);
        }

        // no need to check for missing source node, that'll be handled in ValidateGraphSync

        [Fact]
        public async Task VerifySyncComponent_PropertyMissing_ReturnsFalse()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            bool verified = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_PropertyDifferent_ReturnsFalse()
        {
            const string text = "abc";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\"}}");

            SourceNodeProperties.Add(_fieldName, "some_other_value");

            bool verified = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        private async Task<bool> CallVerifySyncComponent()
        {
            return await TextFieldGraphSyncer.VerifySyncComponent(
                ContentItemField!,
                ContentPartFieldDefinition,
                SourceNode,
                Relationships,
                DestinationNodes,
                GraphSyncHelper);
        }
    }
}
