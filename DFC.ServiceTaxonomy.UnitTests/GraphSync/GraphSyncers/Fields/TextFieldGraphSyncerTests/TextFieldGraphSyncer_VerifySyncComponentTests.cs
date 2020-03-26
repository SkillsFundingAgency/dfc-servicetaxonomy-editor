using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.TextFieldGraphSyncerTests
{
    public class TextFieldGraphSyncer_VerifySyncComponentTests
    {
        public JObject? ContentItemField { get; set; }
        public ContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public INode SourceNode { get; set; }
        public IEnumerable<IRelationship> Relationships { get; set; }
        public IEnumerable<INode> DestinationNodes { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public TextFieldGraphSyncer TextFieldGraphSyncer { get; set; }

        const string _fieldName = "TestTextFieldName";

        public TextFieldGraphSyncer_VerifySyncComponentTests()
        {
            //todo: if this ok, remove interface
            //ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            //A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);
            ContentPartFieldDefinition = new ContentPartFieldDefinition(null, _fieldName, null);

            SourceNode = A.Fake<INode>();
            Relationships = new IRelationship[0];
            DestinationNodes = new INode[0];

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);

            TextFieldGraphSyncer = new TextFieldGraphSyncer();
        }

        private async Task CallVerifySyncComponent()
        {
            await TextFieldGraphSyncer.VerifySyncComponent(
                ContentItemField!,
                ContentPartFieldDefinition,
                SourceNode,
                Relationships,
                DestinationNodes,
                GraphSyncHelper);
        }
    }
}
