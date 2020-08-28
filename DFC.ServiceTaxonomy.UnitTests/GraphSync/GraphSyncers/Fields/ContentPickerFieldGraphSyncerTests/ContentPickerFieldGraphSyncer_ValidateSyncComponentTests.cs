using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.FieldGraphSyncer;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.ContentPickerFieldGraphSyncerTests
{
    public class ContentPickerFieldGraphSyncer_ValidateSyncComponentTests : FieldGraphSyncer_ValidateSyncComponentTests
    {
        public const string ContentKey = "ContentItemIds";

        public ILogger<ContentPickerFieldGraphSyncer> Logger { get; set; }

        public ContentPickerFieldGraphSyncer_ValidateSyncComponentTests()
        {
            Logger = A.Fake<ILogger<ContentPickerFieldGraphSyncer>>();
            ContentFieldGraphSyncer = new ContentPickerFieldGraphSyncer(A.Fake<IPreExistingContentItemVersion>(), Logger, A.Fake<IContentDefinitionManager>());
        }

        //todo: tests
    }
}
