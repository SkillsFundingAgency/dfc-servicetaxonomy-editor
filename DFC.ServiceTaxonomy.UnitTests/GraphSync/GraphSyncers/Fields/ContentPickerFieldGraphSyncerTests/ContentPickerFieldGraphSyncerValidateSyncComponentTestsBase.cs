using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.ContentPickerFieldGraphSyncerTests
{
    public class ContentPickerFieldGraphSyncerValidateSyncComponentTestsBase : FieldGraphSyncer_ValidateSyncComponentTestsBase
    {
        public const string ContentKey = "ContentItemIds";

        public ILogger<ContentPickerFieldGraphSyncer> Logger { get; set; }

        public ContentPickerFieldGraphSyncerValidateSyncComponentTestsBase()
        {
            Logger = A.Fake<ILogger<ContentPickerFieldGraphSyncer>>();
            ContentFieldGraphSyncer = new ContentPickerFieldGraphSyncer(Logger);
        }

        //todo: tests
    }
}
