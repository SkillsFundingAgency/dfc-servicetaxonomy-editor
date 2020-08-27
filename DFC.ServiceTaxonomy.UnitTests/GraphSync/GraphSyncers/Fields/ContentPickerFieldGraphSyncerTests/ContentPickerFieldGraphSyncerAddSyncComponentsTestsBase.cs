using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentFields.Settings;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.ContentPickerFieldGraphSyncerTests
{
    public class ContentPickerFieldGraphSyncerAddSyncComponentsTestsBase : FieldGraphSyncer_AddSyncComponentsTestsBase
    {
        public ILogger<ContentPickerFieldGraphSyncer> Logger { get; set; }
        public ContentPickerFieldSettings ContentPickerFieldSettings { get; set; }

        public ContentPickerFieldGraphSyncerAddSyncComponentsTestsBase()
        {
            Logger = A.Fake<ILogger<ContentPickerFieldGraphSyncer>>();
            ContentFieldGraphSyncer = new ContentPickerFieldGraphSyncer(Logger);

            ContentPickerFieldSettings = A.Fake<ContentPickerFieldSettings>();

            A.CallTo(() => ContentPartFieldDefinition.GetSettings<ContentPickerFieldSettings>())
                .Returns(ContentPickerFieldSettings);
        }

        //todo: tests

         // [Fact]
         // public async Task AddSyncComponents_EmptyContentItemIds_NoRelationshipsAddedToReplaceRelationshipsCommand()
         // {
         //     ContentItemField = JObject.Parse("{\"ContentItemIds\": []}");
         //
         //     await CallAddSyncComponents();
         //
         //     Assert.Empty(ReplaceRelationshipsCommand.Relationships);
         // }
    }
}
