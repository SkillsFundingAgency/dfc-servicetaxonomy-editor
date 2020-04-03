using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.LinkFieldGraphSyncerTests
{
    public class LinkFieldGraphSyncer_VerifySyncComponentTests
    {
        public JObject? ContentItemField { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public INode SourceNode { get; set; }
        public Dictionary<string, object> SourceNodeProperties { get; set; }
        public IEnumerable<IRelationship> Relationships { get; set; }
        public IEnumerable<INode> DestinationNodes { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public LinkFieldGraphSyncer LinkFieldGraphSyncer { get; set; }

        const string _fieldName = "TestLinkFieldName";

        public LinkFieldGraphSyncer_VerifySyncComponentTests()
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

            LinkFieldGraphSyncer = new LinkFieldGraphSyncer();
        }

        [Fact]
        public async Task VerifySyncComponent_TextAndUrlPropertiesCorrect_ReturnsTrue()
        {
            const string text = "abc";
            const string url = "http://example.com/";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\", \"Url\": \"{url}\"}}");

            SourceNodeProperties.Add($"{_fieldName}_text", text);
            SourceNodeProperties.Add($"{_fieldName}_url", url);

            bool verified = await CallVerifySyncComponent();

            Assert.True(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_TextPropertyCorrectUrlPropertyMissing_ReturnsFalse()
        {
            const string text = "abc";
            const string url = "http://example.com/";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\", \"Url\": \"{url}\"}}");

            SourceNodeProperties.Add($"{_fieldName}_text", text);

            bool verified = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_UrlPropertyCorrectTextPropertyMissing_ReturnsFalse()
        {
            const string text = "abc";
            const string url = "http://example.com/";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\", \"Url\": \"{url}\"}}");

            SourceNodeProperties.Add($"{_fieldName}_url", url);

            bool verified = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_BothPropertiesMissing_ReturnsFalse()
        {
            const string text = "abc";
            const string url = "http://example.com/";
            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\", \"Url\": \"{url}\"}}");

            bool verified = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_TextPropertyCorrectUrlPropertyDifferent_ReturnsFalse()
        {
            const string text = "abc";
            const string url = "http://example.com/";
            const string differentUrl = "http://apocalypsenow.com/";

            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\", \"Url\": \"{url}\"}}");

            SourceNodeProperties.Add($"{_fieldName}_text", text);
            SourceNodeProperties.Add($"{_fieldName}_url", differentUrl);

            bool verified = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        [Fact]
        public async Task VerifySyncComponent_UrlPropertyCorrectTextPropertyDifferent_ReturnsFalse()
        {
            const string text = "abc";
            const string url = "http://example.com/";
            const string differentText = "xyz";

            ContentItemField = JObject.Parse($"{{\"Text\": \"{text}\", \"Url\": \"{url}\"}}");

            SourceNodeProperties.Add($"{_fieldName}_text", differentText);
            SourceNodeProperties.Add($"{_fieldName}_url", url);

            bool verified = await CallVerifySyncComponent();

            Assert.False(verified);
        }

        //todo: tests for different types?

        private async Task<bool> CallVerifySyncComponent()
        {
            return await LinkFieldGraphSyncer.VerifySyncComponent(
                ContentItemField!,
                ContentPartFieldDefinition,
                SourceNode,
                Relationships,
                DestinationNodes,
                GraphSyncHelper);
        }
    }
}
