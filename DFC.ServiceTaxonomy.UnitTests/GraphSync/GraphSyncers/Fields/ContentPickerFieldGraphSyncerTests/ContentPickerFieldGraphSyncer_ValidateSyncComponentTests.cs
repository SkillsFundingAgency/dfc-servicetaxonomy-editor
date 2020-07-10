using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.FieldGraphSyncer;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.ContentPickerFieldGraphSyncerTests
{
    public class ContentPickerFieldGraphSyncer_ValidateSyncComponentTests : FieldGraphSyncer_ValidateSyncComponentTests
    {
        public const string ContentKey = "ContentItemIds";

        public ILogger<ContentPickerFieldGraphSyncer> Logger { get; set; }

        public ContentPickerFieldGraphSyncer_ValidateSyncComponentTests()
        {
            Logger = A.Fake<ILogger<ContentPickerFieldGraphSyncer>>();
            ContentFieldGraphSyncer = new ContentPickerFieldGraphSyncer(Logger);
        }

        //todo: tests
    }
}
