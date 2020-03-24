using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields
{
    public class ContentPickerFieldGraphSyncerTests
    {
        public JObject? ContentItemField { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public ContentPickerFieldGraphSyncer ContentPickerFieldGraphSyncer { get; set; }
        public IContentManager ContentManager { get; set; }

        const string _fieldName = "TestField";

        public ContentPickerFieldGraphSyncerTests()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();

            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);

            GraphSyncHelper = A.Fake<IGraphSyncHelper>();
            A.CallTo(() => GraphSyncHelper.PropertyName(_fieldName)).Returns(_fieldName);

            ContentManager = A.Fake<IContentManager>();

            ContentPickerFieldGraphSyncer = new ContentPickerFieldGraphSyncer(ContentManager);
        }

        private async Task CallAddSyncComponents()
        {
            await ContentPickerFieldGraphSyncer.AddSyncComponents(
                ContentItemField!,
                MergeNodeCommand,
                ReplaceRelationshipsCommand,
                ContentPartFieldDefinition,
                GraphSyncHelper);
        }
    }
}
