using System;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using FakeItEasy;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.ContentPickerFieldGraphSyncerTests
{
    public class ContentPickerFieldGraphSyncerValidateSyncComponentTestsBase : FieldGraphSyncer_ValidateSyncComponentTestsBase
    {
        public const string ContentKey = "ContentItemIds";

        public ContentPickerFieldGraphSyncerValidateSyncComponentTestsBase()
        {
            ContentFieldGraphSyncer = new ContentPickerFieldGraphSyncer(A.Fake<IServiceProvider>());
        }

        //todo: tests
    }
}
