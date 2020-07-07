using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers;
using FakeItEasy;
using OrchardCore.ContentFields.Settings;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.ContentPickerFieldGraphSyncerTests
{
    public class ContentPickerFieldGraphSyncer_AddSyncComponentsTests : FieldGraphSyncer_AddSyncComponentsTests
    {
        public ContentPickerFieldSettings ContentPickerFieldSettings { get; set; }

        public ContentPickerFieldGraphSyncer_AddSyncComponentsTests()
        {
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
