using System;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using FakeItEasy;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.ContentPickerFieldGraphSyncerTests
{
    public class ContentPickerFieldGraphSyncerValidateSyncComponentTestsBase : FieldGraphSyncer_ValidateSyncComponentTestsBase
    {
        public const string ContentKey = "ContentItemIds";

        public ContentPickerFieldGraphSyncerValidateSyncComponentTestsBase()
        {
            ContentFieldGraphSyncer = new ContentPickerFieldGraphSyncer(A.Fake<IPreExistingContentItemVersion>(), A.Fake<IContentDefinitionManager>(), A.Fake<IServiceProvider>());
        }

        //todo: tests
    }
}
