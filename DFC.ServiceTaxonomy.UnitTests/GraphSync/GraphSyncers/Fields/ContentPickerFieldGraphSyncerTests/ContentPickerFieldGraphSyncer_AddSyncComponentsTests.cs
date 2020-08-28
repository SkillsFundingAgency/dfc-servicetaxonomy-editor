using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.FieldGraphSyncer;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.ContentPickerFieldGraphSyncerTests
{
    public class ContentPickerFieldGraphSyncer_AddSyncComponentsTests : FieldGraphSyncer_AddSyncComponentsTests
    {
        public ILogger<ContentPickerFieldGraphSyncer> Logger { get; set; }
        public ContentPickerFieldSettings ContentPickerFieldSettings { get; set; }

        public ContentPickerFieldGraphSyncer_AddSyncComponentsTests()
        {
            Logger = A.Fake<ILogger<ContentPickerFieldGraphSyncer>>();
            ContentFieldGraphSyncer = new ContentPickerFieldGraphSyncer(A.Fake<IPreExistingContentItemVersion>(), Logger, A.Fake<IContentDefinitionManager>());

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
