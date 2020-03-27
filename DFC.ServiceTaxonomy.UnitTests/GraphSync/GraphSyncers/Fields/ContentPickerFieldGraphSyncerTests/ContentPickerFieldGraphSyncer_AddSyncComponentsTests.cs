using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.ContentPickerFieldGraphSyncerTests
{
    public class ContentPickerFieldGraphSyncer_AddSyncComponentsTests
    {
        public JObject? ContentItemField { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public ContentPickerFieldGraphSyncer ContentPickerFieldGraphSyncer { get; set; }
        public IContentManager ContentManager { get; set; }
        public ILogger<ContentPickerFieldGraphSyncer> Logger { get; set; }

        const string _fieldName = "TestField";

        public ContentPickerFieldGraphSyncer_AddSyncComponentsTests()
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

            Logger = A.Fake<ILogger<ContentPickerFieldGraphSyncer>>();

            ContentPickerFieldGraphSyncer = new ContentPickerFieldGraphSyncer(ContentManager, Logger);
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
